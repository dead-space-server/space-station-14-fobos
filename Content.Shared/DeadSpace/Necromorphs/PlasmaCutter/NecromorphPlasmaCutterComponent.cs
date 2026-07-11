// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Necromorphs.PlasmaCutter;

/// <summary>
/// Makes a hitscan apply progressive dismemberment or percentage damage to necromorphs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NecromorphPlasmaCutterComponent : Component;

/// <summary>
/// Tracks progressive plasma-cutter injuries on a humanoid necromorph.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NecromorphPlasmaCutterWoundsComponent : Component
{
    [DataField, AutoNetworkedField]
    public int RemovedLegs;
}

/// <summary>
/// A corpse with this marker cannot be transformed or restored by necromorph infection.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NecromorphMissingHeadComponent : Component;

/// <summary>
/// Tracks hits against non-humanoid necromorphs.
/// </summary>
[RegisterComponent]
public sealed partial class NecromorphPlasmaCutterDamageComponent : Component
{
    [DataField]
    public int Hits;
}
