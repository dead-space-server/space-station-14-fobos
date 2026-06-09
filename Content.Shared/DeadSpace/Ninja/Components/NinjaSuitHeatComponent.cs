using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaSuitHeatComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Heat = 0f;

    [DataField]
    public float MaxHeat = 100f;

    [DataField]
    public float HeatRate = 5f;

    [DataField]
    public float CoolRate = 10f;

    [DataField]
    public float EffectsThreshold = 50f;
}