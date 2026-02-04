using Content.Shared._Frostheim.HeatSource;
using Content.Shared.IgnitionSource;

namespace Content.Server._Frostheim.HeatSource;

public sealed class FrostHeatSourceToggleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatSourceComponent, IgnitionEvent>(OnIgnition);
    }

    private void OnIgnition(EntityUid uid, HeatSourceComponent comp, ref IgnitionEvent args)
    {
        comp.Enabled = args.Ignite;
        Dirty(uid, comp);
    }
}
