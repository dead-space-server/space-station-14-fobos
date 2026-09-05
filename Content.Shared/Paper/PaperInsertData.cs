// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.CrewManifest;
using Robust.Shared.Serialization;

namespace Content.Shared.Paper;

// DS14-start
[Serializable, NetSerializable]
public sealed class PaperInsertDataRequestMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PaperInsertDataResponseMessage : BoundUserInterfaceMessage
{
    public readonly string? StationName;

    public readonly string RoundDateTime;

    public readonly string CharacterName;

    public readonly string? CharacterJob;

    public readonly List<CrewManifestEntry> Manifest;

    public PaperInsertDataResponseMessage(
        string? stationName,
        string roundDateTime,
        string characterName,
        string? characterJob,
        List<CrewManifestEntry> manifest)
    {
        StationName = stationName;
        RoundDateTime = roundDateTime;
        CharacterName = characterName;
        CharacterJob = characterJob;
        Manifest = manifest;
    }
}
// DS14-end
