// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Atmos.Hallucinations;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.Atmos;
using Content.Shared.Radiation.Components;
using Robust.Shared.Map;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    [Dependency] public readonly ParacusiaHallucinationsSystem Hallucinations = default!;

    private const string RadiationPulsePrototype = "RadiationPulseSilent";

    public bool TryGetGasReactionCoordinates(IGasMixtureHolder? holder, out MapCoordinates coords)
    {
        coords = MapCoordinates.Nullspace;
        switch (holder)
        {
            case TileAtmosphere tile:
                coords = GetTileWorldCoordinates(tile);
                break;
            case IPipeNet pipeNet:
                foreach (var node in pipeNet.Nodes)
                {
                    if (!Exists(node.Owner))
                        continue;

                    coords = _transformSystem.GetMapCoordinates(node.Owner);
                    break;
                }
                break;
        }

        return coords.MapId != MapId.Nullspace;
    }

    public void EmitRadiationPulse(IGasMixtureHolder? holder, float intensity, float slope = 0.5f)
    {
        if (intensity <= 0f)
            return;

        if (!TryGetGasReactionCoordinates(holder, out var coords))
            return;

        var pulse = Spawn(RadiationPulsePrototype, coords);
        if (TryComp<RadiationSourceComponent>(pulse, out var source))
        {
            source.Intensity = intensity;
            source.Slope = slope;
        }
    }
}
