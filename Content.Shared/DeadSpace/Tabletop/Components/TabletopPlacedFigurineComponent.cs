using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Tabletop.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TabletopPlacedFigurineComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public string? Prototype;
}
