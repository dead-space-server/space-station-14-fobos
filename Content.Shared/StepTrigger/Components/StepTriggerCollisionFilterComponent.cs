using Content.Shared.StepTrigger.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.StepTrigger.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(StepTriggerCollisionFilterSystem))]
public sealed partial class StepTriggerCollisionFilterComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IgnoreCanMoveInAir;

    [DataField, AutoNetworkedField]
    public bool IgnoreLavalandChasmImmune;

    [DataField, AutoNetworkedField]
    public bool IgnoreLavalandFauna;
}
