// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAnnihilationComponent : Component
{
    [DataField]
    public EntProtoId ActionAnnihilation = "ActionShadowlingAnnihilation";

    [DataField]
    public string ImmunePrototypeId = "MobHumanDeathSquadUnit";

    [DataField]
    public EntityUid? ActionAnnihilationEntity;
}

public sealed partial class ShadowlingAnnihilationEvent : EntityTargetActionEvent { }