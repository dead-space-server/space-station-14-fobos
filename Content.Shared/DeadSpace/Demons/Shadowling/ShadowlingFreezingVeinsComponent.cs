// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingFreezingVeinsComponent : Component
{
    [DataField] public EntProtoId ActionFreezingVeins = "ActionShadowlingFreezingVeins";
    [DataField] public EntityUid? ActionFreezingVeinsEntity;
    [DataField] public int RequiredSlaves = 5;
    [DataField] public float DamageCold = 25f;
    [DataField] public float TemperatureSet = 233.15f;
    [DataField] public string ImmunePrototypeId = "MobHumanDeathSquadUnit";
}

public sealed partial class ShadowlingFreezingVeinsEvent : EntityTargetActionEvent { }