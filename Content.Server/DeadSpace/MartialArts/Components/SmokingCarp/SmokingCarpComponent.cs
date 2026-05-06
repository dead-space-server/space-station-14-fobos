// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
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
    public SmokingCarpList? SelectedCombo; // Выбранное комбо, которое меняется при вызове события

    public readonly List<EntProtoId> BaseSmokingCarp = new() // Список всех Action, которые будут выдаваться пользователю
    {
        "ActionPowerPunchCarp",
        "ActionSmokePunchCarp",
        "ActionTripPunchCarp",
        "ActionReflectCarp",
    };

    [DataField]
    public SmokingCarpParams Params; // Передача всех переменных и хранение всех переменных, хранится в MartialArtsTrainingComponent
}

[RegisterComponent]
public sealed partial class SmokingCarpPacifiedComponent : Component { }

public enum SmokingCarpList
{
    PowerPunch,
    SmokePunch,
}