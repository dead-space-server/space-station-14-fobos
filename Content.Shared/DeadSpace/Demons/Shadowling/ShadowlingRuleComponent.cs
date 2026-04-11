using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField] public int TargetSlaves = 15;
}
