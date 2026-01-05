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

    public bool CnowThisSkill(EntityUid uid, string prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        return component.Skills.TryGetValue(prototypeId, out var progress) && progress >= 1f;
    }

    public float GetSkillProgress(EntityUid uid, string prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return 0f;

        if (!component.Skills.TryGetValue(prototypeId, out var progress))
            return 0f;

        return progress;
    }

    public bool CanLearn(EntityUid uid, string prototypeId, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (!_prototypeManager.TryIndex<SkillPrototype>(prototypeId, out var prototype))
        {
            _sawmill.Warning($"Прототип навыка {prototypeId} не найден");
            return false;
        }

        if (!LimitControl(uid, prototype, component))
            return false;

        return !CnowThisSkill(uid, prototypeId, component);
    }

    public void AddSkillProgress(EntityUid uid, string prototypeId, float progress, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (!_prototypeManager.TryIndex<SkillPrototype>(prototypeId, out var prototype))
        {
            _sawmill.Warning($"Прототип навыка {prototypeId} не найден");
            return;
        }

        if (!LimitControl(uid, prototype, component))
            return;

        if (component.Skills.TryGetValue(prototypeId, out var currentProgress))
            component.Skills[prototypeId] = Math.Min(1f, currentProgress + progress);
        else
            component.Skills[prototypeId] = Math.Min(1f, progress);
    }

    private bool LimitControl(EntityUid uid, SkillPrototype prototype, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        foreach (var (key, addValue) in prototype.AddLimit)
        {
            var current = GetCurrentLimit(uid, key, component);

            if (!component.MaxLimit.TryGetValue(key, out var maxLimit))
            {
                maxLimit = component.DefaultMaxLimit;
                component.MaxLimit[key] = maxLimit;
            }

            if (current + addValue > maxLimit)
                return false;
        }

        return true;
    }

    private int GetCurrentLimit(EntityUid uid, string key, SkillComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return 0;

        int result = 0;

        foreach (var (limitKey, addValue) in component.Skills)
        {
            if (limitKey != key)
                continue;

            if (!_prototypeManager.TryIndex<SkillPrototype>(key, out var proto))
                continue;

            result += proto.AddLimit[key];
        }

        return result;
    }


}
