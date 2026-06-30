namespace Content.Shared.DeadSpace.FakeDescription;

[RegisterComponent]
public sealed partial class FakeInsulatedComponent : Component
{
    [DataField]
    public bool Insulated = true;
}
