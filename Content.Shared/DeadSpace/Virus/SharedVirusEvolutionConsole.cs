// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Prototypes;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Virus;

[Serializable, NetSerializable]
public sealed class VirusEvolutionConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public int MutationPoints { get; }
    public int SymptomsCount { get; }
    public int BodyCount { get; }
    public bool DataServerConnected { get; }
    public bool SolutionAnalyzerConnected { get; }
    public bool DataServerInRange { get; }
    public bool SolutionAnalyzerInRange { get; }
    public bool HasVirus { get; }
    public List<ProtoId<VirusSymptomPrototype>> ActiveSymptoms = new();
    public List<ProtoId<BodyPrototype>> BodyWhitelist = new();

    public VirusEvolutionConsoleBoundUserInterfaceState(
        int mutationPoints,
        bool dataServerConnected,
        bool solutionAnalyzerConnected,
        bool dataServerInRange,
        bool solutionAnalyzerInRange,
        bool hasVirus = false,
        List<ProtoId<VirusSymptomPrototype>>? activeSymptoms = null,
        List<ProtoId<BodyPrototype>>? bodyWhitelist = null)
    {
        MutationPoints = mutationPoints;
        DataServerConnected = dataServerConnected;
        SolutionAnalyzerConnected = solutionAnalyzerConnected;
        DataServerInRange = dataServerInRange;
        SolutionAnalyzerInRange = solutionAnalyzerInRange;
        ActiveSymptoms = activeSymptoms ?? new List<ProtoId<VirusSymptomPrototype>>();
        BodyWhitelist = bodyWhitelist ?? new List<ProtoId<BodyPrototype>>();
        HasVirus = hasVirus;
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

[Serializable, NetSerializable]
public enum VirusEvolutionConsoleUiKey : byte
{
    Key
}