// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Demons.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingDarkMindComponent : Component
{
    [DataField]
    public EntProtoId ActionDarkMind = "ActionShadowlingDarkMind";

    [DataField]
    public EntityUid? ActionDarkMindEntity;
}

public sealed partial class ShadowlingDarkMindEvent : InstantActionEvent { }