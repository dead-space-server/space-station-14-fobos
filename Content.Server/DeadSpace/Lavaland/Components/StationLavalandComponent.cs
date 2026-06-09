using System.Threading;
using System.Threading.Tasks;
using Content.Shared.DeadSpace.Lavaland;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent, Access(typeof(LavalandSystem))]
public sealed partial class StationLavalandComponent : Component
{
    [DataField]
    public List<ProtoId<LavalandPlanetPrototype>> Planets = new()
    {
        "Lavaland",
    };

    [DataField]
    public EntityUid? GeneratedMap;

    public CancellationTokenSource? GenerationCancel;

    public Task? GenerationTask;
}
