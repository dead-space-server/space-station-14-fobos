namespace Content.Shared.DeadSpace.ShieldDash.Components;

[RegisterComponent]
public sealed partial class ShieldDashUserComponent : Component
{
    [DataField]
    public EntityUid? Shield;
}