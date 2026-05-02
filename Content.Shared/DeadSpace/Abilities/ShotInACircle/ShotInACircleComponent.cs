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