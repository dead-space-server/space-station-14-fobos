// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingReRevealComponent : Component
{
    [DataField] public EntProtoId ActionReReveal = "ActionShadowlingReReveal";
    [DataField] public EntityUid? ActionReRevealEntity;
    [DataField] public int RequiredSlaves = 7;
    [DataField] public float Duration = 5f;
}

public sealed partial class ShadowlingReRevealEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class ShadowlingReRevealDoAfterEvent : SimpleDoAfterEvent { }