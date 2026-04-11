using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingRecruitObjectiveComponent : Component
{
    [DataField]
    public int TargetCount = 15;
}
