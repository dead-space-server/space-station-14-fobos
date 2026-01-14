// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Virus;

[Serializable, NetSerializable]
public enum VirusSolutionAnalyzerVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum VirusSolutionContainerAnalyzerVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum VirusSolutionAnalyzerStatus : byte
{
    On,
    Off,
    Scanning,
    Denial,
    Successfully
}

[Serializable, NetSerializable]
public enum VirusSolutionContainerAnalyzerStatus : byte
{
    Empty,
    Fill
}
