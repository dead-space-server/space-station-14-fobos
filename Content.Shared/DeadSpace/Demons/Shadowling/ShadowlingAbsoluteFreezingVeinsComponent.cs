// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAbsoluteFreezingVeinsComponent : Component
{
    [DataField] public EntProtoId ActionAbsoluteFreezingVeins = "ActionShadowlingAbsoluteFreezingVeins";
    [DataField] public EntityUid? ActionAbsoluteFreezingVeinsEntity;
    [DataField] public float DamageCold = 50f;
    [DataField] public float TemperatureSet = 193.15f;
    [DataField] public string ImmunePrototypeId = "MobHumanDeathSquadUnit";
}

public sealed partial class ShadowlingAbsoluteFreezingVeinsEvent : EntityTargetActionEvent { }