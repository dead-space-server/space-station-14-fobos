using Content.Server.DeadSpace.MartialArts.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;

[RegisterComponent]
public sealed partial class SmokingCarpTripPunchComponent : Component
{
    [DataField]
    public EntProtoId? SelfEffect = "EffectTripPunchCarp";

    [DataField]
    public SoundSpecifier? TripSound = new SoundPathSpecifier("/Audio/_DeadSpace/SmokingCarp/sound_items_weapons_slam.ogg");

    [DataField]
    public float Range = 1.0f;

    [DataField]
    public float ParalyzeTime = 1.2f;
}

[RegisterComponent]
public sealed partial class SmokingCarpComponent : Component
{
    [DataField]
    public SmokingCarpList? SelectedCombo;

    public readonly List<EntProtoId> BaseSmokingCarp = new()
    {
        "ActionPowerPunchCarp",
        "ActionSmokePunchCarp",
        "ActionTripPunchCarp",
        "ActionReflectCarp",
    };

    public readonly List<EntityUid> SmokeCarpActionEntities = new()
    {
    };

    [DataField]
    public MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.SmokingCarp;

    [DataField]
    public SmokingCarpParams Params;
}

public enum SmokingCarpList
{
    PowerPunch,
    SmokePunch,
}
