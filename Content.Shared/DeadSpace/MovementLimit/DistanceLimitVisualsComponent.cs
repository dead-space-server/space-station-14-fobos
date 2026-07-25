// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared.DeadSpace.MovementLimit;

/// <summary>
/// Added to entities to display a visual range limit overlay to the client.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DistanceLimitVisualsComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Origin;

    [DataField, AutoNetworkedField]
    public float MaxDistance = 5f;

    [DataField, AutoNetworkedField]
    public Color Color = Color.FromHex("#90EE90"); // Light green
}
