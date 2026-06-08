using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaSmokeAbilityComponent : Component
{
    [DataField]
    public EntProtoId ActionSmoke;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionSmokeEntity;

    [DataField]
    public EntProtoId ActionAutoSmoke;

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
    public float Charge;
    /// <summary>
    /// Will the ability be activated by others?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoMode = false;
}

public sealed partial class NinjaSmokeAbilityActionEvent : InstantActionEvent;