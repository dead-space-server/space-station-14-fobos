

[Serializable, NetSerializable]
public enum VirusDiagnoserVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum VirusDiagnoserStatus : byte
{
    Off,
    On,
    Printing,
    Scanning,
    Deniel,
    Successfully,
    GenerateVirus
}