using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaSmokeAbilityComponent : Component
{
    [DataField]
    public EntProtoId ActionSmoke = "NinjaSmokeAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionSmokeEntity;

    [DataField]
    public EntProtoId ActionAutoSmoke = "ToggleNinjaAutoSmokeAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionAutoSmokeEntity;

    /// <summary>
    /// How long the smoke stays for, after it has spread (in seconds).
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Solution Solution = new();

    /// <summary>
    /// Battery charge used to create smoke.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeAutoMode;

    /// <summary>
    /// How long the smoke stays in auto mode for, after it has spread (in seconds).
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DurationAutoMode = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How much the smoke will spread in auto mode.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public int SpreadAmountAutoMode;

    /// <summary>
    /// Battery charge used to create smoke by auto mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeAutoMode;

    /// <summary>
    /// Solution to add to each smoke cloud in auto mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Solution SolutionAutoMode = new();

    /// <summary>
    /// Will the ability be activated by others?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoMode = false;
}

public sealed partial class NinjaSmokeAbilityActionEvent : InstantActionEvent;

public sealed partial class NinjaToggleAutoSmokeActionEvent : InstantActionEvent;