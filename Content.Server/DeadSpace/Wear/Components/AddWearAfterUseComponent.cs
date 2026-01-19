// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Server.DeadSpace.Wear.Components;

[RegisterComponent]
public sealed partial class AddWearAfterUseComponent : Component
{
    /// <summary>
    ///     Урон при использовании
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new DamageSpecifier();

    [DataField]
    public WearTrigger Triggers = WearTrigger.None;

    [DataField]
    public EntityWhitelist? Whitelist;
}

/// <summary>
///     Какие типы действий могут вызывать износ предмета.
/// </summary>
[Flags]
public enum WearTrigger
{
    None = 0,
    MeleeHit = 1 << 0,
    Interact = 1 << 1,
    TileTool = 1 << 2,
    Learn = 1 << 3,
    RCD = 1 << 4,
}