// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Virus;

[Serializable, NetSerializable]
public sealed class VirusEvolutionConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly int MutationPoints;
    public readonly VirusData Data;

    public VirusEvolutionConsoleBoundUserInterfaceState(
        int mutationPoints,
        VirusData data)
    {
        MutationPoints = mutationPoints;
        Data = data;
    }
}
