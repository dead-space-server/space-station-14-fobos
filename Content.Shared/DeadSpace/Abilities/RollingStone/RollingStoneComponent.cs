// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.Abilities;

public sealed partial class RollingStoneActionEvent : InstantActionEvent
{
    [DataField]
    public float Duration = 8f;

    [DataField]
    public float Speed = 8f;

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public SoundSpecifier? HitSound;
}