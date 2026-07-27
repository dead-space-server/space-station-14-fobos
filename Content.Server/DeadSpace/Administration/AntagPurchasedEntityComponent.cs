// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Antag;
using Content.Server.Implants;
using Content.Server.Store.Systems;

namespace Content.Server.DeadSpace.Administration;

/// <summary>
/// Keeps the antagonist owner of a granted item or uplink purchase even when the entity moves between containers.
/// </summary>
[RegisterComponent, Access(typeof(AntagSelectionSystem), typeof(StoreSystem), typeof(ImplanterSystem))]
public sealed partial class AntagPurchasedEntityComponent : Component
{
    public EntityUid MindId;
}
