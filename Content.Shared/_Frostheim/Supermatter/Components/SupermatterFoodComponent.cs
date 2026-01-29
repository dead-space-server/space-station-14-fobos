namespace Content.Shared._Frostheim.Supermatter.Components;

[RegisterComponent]
public sealed partial class SupermatterFoodComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int Energy { get; set; } = 1;
}
