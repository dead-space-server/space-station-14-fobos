using Content.Shared.Actions;
using Content.Shared.DeadSpace.Ninja.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Ninja.Components;

/// <summary>
/// Component for ninja suit abilities and power consumption.
/// As an implementation detail, dashing with katana is a suit action which isn't ideal.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedNinjaSuitSystem))]
public sealed partial class NinjaSuitComponent : Component
{
    /// <summary>
    /// Sound played when a ninja is hit while cloaked.
    /// </summary>
    [DataField]
    public SoundSpecifier RevealSound = new SoundPathSpecifier("/Audio/Effects/chime.ogg");

    /// <summary>
    /// ID of the use delay to disable all ninja abilities.
    /// </summary>
    [DataField]
    public string DisableDelayId = "suit_powers";

    /// <summary>
    /// The action id for recalling a bound energy katana
    /// </summary>
    [DataField]
    public EntProtoId RecallKatanaAction = "ActionRecallKatana";

    [DataField, AutoNetworkedField]
    public EntityUid? RecallKatanaActionEntity;

    [DataField]
    public EntProtoId OpenSpiderOSAction = "SpiderOSAction";

    [DataField, AutoNetworkedField]
    public EntityUid? OpenSpiderOSActionEntity;

    /// <summary>
    /// Battery charge used per tile the katana teleported.
    /// Uses 1% of a default battery per tile.
    /// </summary>
    [DataField]
    public float RecallCharge = 3.6f;

    // DS14-start
    /// <summary>
    /// If the katana recall cost would be equal to or higher than the current battery capacity,
    /// recalling it costs this fraction of the current battery capacity instead.
    /// </summary>
    [DataField]
    public float RecallOverMaxChargeRatio = 0.9f;
    // DS14-end

    /// <summary>
    /// The action id for creating an EMP burst
    /// </summary>
    [DataField]
    public EntProtoId EmpAction = "ActionNinjaEmp";

    [DataField, AutoNetworkedField]
    public EntityUid? EmpActionEntity;

    /// <summary>
    /// Battery charge used to create an EMP burst. Can do it 2 times on a small-capacity power cell.
    /// </summary>
    [DataField]
    public float EmpCharge = 180f;

    // TODO: EmpOnTrigger bruh

    /// <summary>
    /// Range of the EMP in tiles.
    /// </summary>
    [DataField]
    public float EmpRange = 6f;

    /// <summary>
    /// Power consumed from batteries by the EMP
    /// </summary>
    [DataField]
    public float EmpConsumption = 100000f;

    /// <summary>
    /// How long the EMP effects last for
    /// </summary>
    [DataField]
    public TimeSpan EmpDuration = TimeSpan.FromSeconds(60);
}
//DS-14 start
[Serializable, NetSerializable]
public enum SpiderOSUiKey
{
    Key,
}
//DS-14 end

public sealed partial class RecallKatanaEvent : InstantActionEvent;
public sealed partial class NinjaEmpEvent : InstantActionEvent;
public sealed partial class OpenSpiderOSEvent : InstantActionEvent;
