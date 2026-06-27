namespace Content.Shared.DeadSpace.FakeDescription;

[RegisterComponent]
public sealed partial class FakeClothingSlowOnDamageModifierComponent : Component
{
    [DataField]
    public float Modifier = 1;
}
