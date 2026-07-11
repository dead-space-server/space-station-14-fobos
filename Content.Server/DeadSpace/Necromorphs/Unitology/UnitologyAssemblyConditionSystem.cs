// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Necromorphs.Unitology.Components;
using Content.Shared.DeadSpace.Necromorphs.Unitology.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.DeadSpace.Necromorphs.Unitology;

public sealed class UnitologyAssemblyConditionSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UnitologyAssemblyConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<UnitologyAssemblyConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var total = 0;
        var slaves = AllEntityQuery<UnitologyEnslavedComponent>();
        while (slaves.MoveNext(out _, out _))
            total++;

        if (total == 0)
        {
            args.Progress = 0f;
            return;
        }

        var nearby = 0;
        var heads = AllEntityQuery<UnitologyHeadComponent>();
        while (heads.MoveNext(out var head, out _))
        {
            nearby = Math.Max(nearby,
                _lookup.GetEntitiesInRange<UnitologyEnslavedComponent>(Transform(head).Coordinates, ent.Comp.Range).Count);
        }

        args.Progress = MathF.Min((float) nearby / total, 1f);
    }
}
