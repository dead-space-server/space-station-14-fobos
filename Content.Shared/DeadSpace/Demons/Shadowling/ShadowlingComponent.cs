// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingComponent : Component
{
    [DataField("passiveHealing")] public DamageSpecifier PassiveHealing = new();
    [DataField("healingInterval")] public float HealingInterval = 1.0f;
    [DataField("speedMultiplier")] public float SpeedMultiplier = 1.25f;
    [DataField("threshold")] public float Threshold = 0.5f;

    [ViewVariables] public float Accumulator = 0f;
    [ViewVariables] public bool IsInDarkness = false;
}