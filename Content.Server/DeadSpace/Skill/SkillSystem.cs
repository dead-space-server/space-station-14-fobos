// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Medieval.Skills;
using Content.Shared.DeadSpace.Medieval.Skills.Prototypes;
using Content.Shared.DeadSpace.Medieval.Skills.Components;

namespace Content.Server.DeadSpace.Medieval.Skill;

public sealed class SkillSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("SkillSystem");

        SubscribeLocalEvent<SkillComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<SkillComponent> entity, ref ComponentInit args)
    {
        if (!_prototypeManager.TryIndex(entity.Comp.Group, out var group))
            return;

        foreach (var skill in group.Skills)
        {
            if (!entity.Comp.Skills.ContainsKey(skill))
                entity.Comp.Skills[skill] = 0f;
        }
    }

    public SkillInfo? GetSkillInfo(EntityUid uid, string prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return null;

        if (!_prototypeManager.TryIndex<SkillPrototype>(prototypeId, out var skillPrototype) || skillPrototype == null)
        {
            _sawmill.Warning($"Прототип навыка {prototypeId} не найден");
            return null;
        }

        if (!component.Skills.TryGetValue(prototypeId, out var progress))
        {
            _sawmill.Warning($"Не удалось получить прогресс изучения навыка");
            return null;
        }

        SkillInfo skill = new SkillInfo(
            skillPrototype.Name,
            skillPrototype.Description,
            skillPrototype.Icon,
            progress
        );

        return skill;
    }

    public bool CnowThisSkill(EntityUid uid, ProtoId<SkillPrototype> prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        return component.Skills.TryGetValue(prototypeId, out var progress) && progress >= 1f;
    }

    public float GetSkillProgress(EntityUid uid, ProtoId<SkillPrototype> prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return 0f;

        if (!component.Skills.TryGetValue(prototypeId, out var progress))
            return 0f;

        return progress;
    }

    public bool CanLearn(EntityUid uid, ProtoId<SkillPrototype> prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (!_prototypeManager.TryIndex(prototypeId, out var prototype))
        {
            _sawmill.Warning($"Прототип навыка {prototypeId} не найден");
            return false;
        }

        return !CnowThisSkill(uid, prototypeId, component);
    }

    public void AddSkillProgress(EntityUid uid, ProtoId<SkillPrototype> prototypeId, float progress, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (!_prototypeManager.TryIndex(prototypeId, out var prototype))
        {
            _sawmill.Warning($"Прототип навыка {prototypeId} не найден");
            return;
        }

        if (component.Skills.TryGetValue(prototypeId, out var currentProgress))
            component.Skills[prototypeId] = Math.Min(1f, currentProgress + progress);
        else
            component.Skills[prototypeId] = Math.Min(1f, progress);
    }


}
