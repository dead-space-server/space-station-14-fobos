// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Weapons.Ranged;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunUsageBlockerSystem))]
public sealed partial class GunUsageBlockerComponent : Component;
