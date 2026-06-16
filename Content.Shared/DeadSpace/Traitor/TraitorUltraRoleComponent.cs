// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Traitor;

/// <summary>
/// Added to mind role entities to tag traitors that accepted the Ultra escalation contract.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TraitorUltraRoleComponent : BaseMindRoleComponent;
