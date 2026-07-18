// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Weapons.Ranged;

/// <summary>
/// Transient marker used only while one execution shot is processed by GunSystem.
/// </summary>
[RegisterComponent]
public sealed partial class GunExecutionShotComponent : Component;

/// <summary>
/// Permanently blocks firearm suicide for this body after a failed courage check.
/// </summary>
[RegisterComponent]
public sealed partial class GunSuicideBlockedComponent : Component;
