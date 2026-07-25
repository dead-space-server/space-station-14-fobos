// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Alerts;

/// <summary>
/// Relays the health alert of a linked entity (e.g. Host) to this entity (e.g. Holoparasite).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HealthAlertRelayComponent : Component
{
    public override bool SendOnlyToOwner => true;

    /// <summary>
    /// The entity whose health alerts are being relayed to this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedEntity;
}
