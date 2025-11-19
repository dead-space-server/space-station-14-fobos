// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.Virus;

[Serializable, NetSerializable]
public enum VirusDiagnoserConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VirusDiagnoserConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<string> Strains = new();
    public readonly bool DiagnoserConnected;
    public readonly bool DataServerConnected;
    public readonly bool DiagnoserInRange;
    public readonly bool DataServerInRange;
    public VirusDiagnoserConsoleBoundUserInterfaceState(
        List<string>? strains = null,
        bool diagnoserConnected = false,
        bool dataServerConnected = false,
        bool diagnoserInRange = false,
        bool dataServerInRange = false)
    {
        Strains = strains ?? new List<string>();
        DiagnoserConnected = diagnoserConnected;
        DataServerConnected = dataServerConnected;
        DiagnoserInRange = diagnoserInRange;
        DataServerInRange = dataServerInRange;
    }
}

[Serializable, NetSerializable]
public enum UiButton : byte
{
    GenerateVirus,
    PrintReport,
    ScanVirus,
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