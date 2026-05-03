// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Abilities;

public sealed partial class ShotInACircleActionEvent : InstantActionEvent
{
    [DataField]
    public int Count = 8;

    [DataField]
    public EntProtoId Entity = "MeteorSmall";

    [DataField]
    public float ProjectileSpeed = 15f;

    [DataField]
    public float Offset = 1f;
}