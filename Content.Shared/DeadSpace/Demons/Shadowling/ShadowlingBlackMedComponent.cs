// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlackMedComponent : Component
{
    [DataField] public EntProtoId ActionBlackMed = "ActionShadowlingBlackMed";
    [DataField] public EntityUid? ActionBlackMedEntity;

    [DataField] public int RequiredSlaves = 9;
}

public sealed partial class ShadowlingBlackMedEvent : EntityTargetActionEvent { }