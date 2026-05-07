// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingSmokeActionComponent : Component
{
    [DataField] public EntProtoId ActionSmoke = "ActionShadowlingSmoke";
    [DataField] public EntityUid? ActionSmokeEntity;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float SmokeDuration = 20f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int SmokeSpread = 30;
}

public sealed partial class ShadowlingSmokeActionEvent : InstantActionEvent { }