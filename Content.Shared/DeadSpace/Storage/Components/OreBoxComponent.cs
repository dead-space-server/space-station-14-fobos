using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Storage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class OreBoxComponent : Component
{
    [DataField]
    public EntityUid? TransferTarget;
}