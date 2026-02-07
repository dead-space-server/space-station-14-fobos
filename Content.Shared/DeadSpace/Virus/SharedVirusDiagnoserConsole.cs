// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Virus;

[Serializable, NetSerializable]
public enum VirusDiagnoserConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VirusDiagnoserConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<VirusStrainRecord> Strains = new();
    public int Points { get; }
    public bool DiagnoserConnected { get; }
    public bool DataServerConnected { get; }
    public bool SolutionAnalyzerConnected { get; }
    public bool DiagnoserInRange { get; }
    public bool DataServerInRange { get; }
    public bool SolutionAnalyzerInRange { get; }
    public VirusDiagnoserConsoleBoundUserInterfaceState(
        List<VirusStrainRecord>? strains = null,
        int points = 0,
        bool diagnoserConnected = false,
        bool dataServerConnected = false,
        bool solutionAnalyzerConnected = false,
        bool diagnoserInRange = false,
        bool dataServerInRange = false,
        bool solutionAnalyzerInRange = false)
    {
        Strains = strains ?? new List<VirusStrainRecord>();
        Points = points;
        DiagnoserConnected = diagnoserConnected;
        DataServerConnected = dataServerConnected;
        SolutionAnalyzerConnected = solutionAnalyzerConnected;
        DiagnoserInRange = diagnoserInRange;
        DataServerInRange = dataServerInRange;
        SolutionAnalyzerInRange = solutionAnalyzerInRange;
    }
}


[Serializable, NetSerializable]
public readonly struct VirusStrainRecord : IEquatable<VirusStrainRecord>
{
    public readonly string Strain;
    public readonly string Time;

    public VirusStrainRecord(string strain, string time)
    {
        Strain = strain;
        Time = time;
    }

    public bool Equals(VirusStrainRecord other) =>
        Strain == other.Strain && Time == other.Time;

    public override bool Equals(object? obj) =>
        obj is VirusStrainRecord other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Strain, Time);
}


[Serializable, NetSerializable]
public enum UiButton : byte
{
    GenerateVirus,
    PrintReport,
    ScanVirus,
    StartAnalys,
    DeleteData
}

[Serializable, NetSerializable]
public sealed class UiButtonPressedMessage : BoundUserInterfaceMessage
{
    public readonly UiButton Button;
    public string? Strain { get; } = null;

    public UiButtonPressedMessage(UiButton button, string? strain)
    {
        Button = button;
        Strain = strain;
    }
}
