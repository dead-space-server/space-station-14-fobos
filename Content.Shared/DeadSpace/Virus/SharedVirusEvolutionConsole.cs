// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Prototypes;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Virus;

[Serializable, NetSerializable]
public sealed class VirusEvolutionConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly int MutationPoints;

    public readonly bool DataServerConnected;
    public readonly bool SolutionAnalyzerConnected;
    public readonly bool DataServerInRange;
    public readonly bool SolutionAnalyzerInRange;
    public readonly List<ProtoId<VirusSymptomPrototype>>? ActiveSymptoms = null;
    public readonly List<ProtoId<BodyPrototype>>? BodyWhitelist = null;

    public VirusEvolutionConsoleBoundUserInterfaceState(
        int mutationPoints,
        bool dataServerConnected,
        bool solutionAnalyzerConnected,
        bool dataServerInRange,
        bool solutionAnalyzerInRange,
        List<ProtoId<VirusSymptomPrototype>>? activeSymptoms = null,
        List<ProtoId<BodyPrototype>>? bodyWhitelist = null)
    {
        MutationPoints = mutationPoints;
        DataServerConnected = dataServerConnected;
        SolutionAnalyzerConnected = solutionAnalyzerConnected;
        DataServerInRange = dataServerInRange;
        SolutionAnalyzerInRange = solutionAnalyzerInRange;
        ActiveSymptoms = activeSymptoms;
        BodyWhitelist = bodyWhitelist;
    }
}


[Serializable, NetSerializable]
public sealed class EvolutionConsoleUiButtonPressedMessage : BoundUserInterfaceMessage
{
    public readonly EvolutionConsoleUiButton Button;
    public string? NewSymptom { get; } = null;
    public string? NewBodie { get; } = null;

    public EvolutionConsoleUiButtonPressedMessage(
        EvolutionConsoleUiButton button,
        string? newSymptom = null,
        string? newBodie = null
        )
    {
        Button = button;
        NewSymptom = newSymptom;
        NewBodie = newBodie;
    }
}


[Serializable, NetSerializable]
public enum EvolutionConsoleUiButton : byte
{
    EvolutionSymptom,
    EvolutionBody
}