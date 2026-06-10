// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShadowlingRevealComponent : Component
{
    [DataField] public EntProtoId ActionReveal = "ActionShadowlingReveal";
    [DataField, AutoNetworkedField] public EntityUid? ActionRevealEntity;
    [DataField] public float Duration = 8.51f;
}

public sealed partial class ShadowlingRevealEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class ShadowlingRevealDoAfterEvent : SimpleDoAfterEvent { }