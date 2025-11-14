
namespace Content.Shared.Virus;

[Serializable, NetSerializable]
public enum VirusDiagnoserConsole : byte
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
    public VirusDiagnoserConsoleBoundUserInterfaceState( )
    {

    }
}