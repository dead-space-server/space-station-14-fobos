using Content.Shared.Containers.ItemSlots;

namespace Content.Shared.DeadSpace.AppHub;

[RegisterComponent]
public sealed partial class AppHubComponent : Component
{
    public const string PdaSlotId = "AppHub-PdaSlot";

    [DataField]
    public ItemSlot PdaSlot = new();

    [DataField]
    public string SelectedCategory = "All";
}
