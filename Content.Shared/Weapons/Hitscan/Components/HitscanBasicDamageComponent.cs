using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Content.Shared.Whitelist;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// Hitscan entities that have this component will do the damage specified to hit targets (Who didn't reflect it).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HitscanBasicDamageComponent : Component
{
    /// <summary>
    /// How much damage the hitscan weapon will do when hitting a target.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage;

    // DS14-start: preserve armor-piercing ballistic ammo behavior on hitscan bullets.
    [DataField]
    public bool IgnoreResistances;

    /// <summary>
    /// Optional target filters. A matching blacklist blocks damage; a whitelist requires a match.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
    // DS14-end
}
