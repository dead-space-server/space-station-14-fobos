using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Frostheim;

namespace Content.Server._Frostheim;

public sealed class HeatSourceSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatSourceComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (!comp.Enabled)
                continue;

            comp.NextUpdate -= frameTime;
            if (comp.NextUpdate > 0)
                continue;

            comp.NextUpdate = comp.UpdateInterval;

            var energy = comp.Power * comp.UpdateInterval;

            var entities = _lookup.GetEntitiesInRange<TemperatureComponent>(xform.Coordinates, comp.Radius, LookupFlags.Dynamic | LookupFlags.Sundries);
            foreach (var target in entities)
            {
                if (target.Owner == uid)
                    continue;

                if (target.Comp.CurrentTemperature >= comp.TargetTemperature)
                    continue;

                _temperature.ChangeHeat(target, energy);
            }
        }
    }
}
