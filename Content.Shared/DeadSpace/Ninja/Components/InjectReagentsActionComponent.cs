using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent]
public sealed partial class InjectReagentsActionComponent : Component
{
    [DataField]
    public string? Popup;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();

    [DataField]
    public SoundSpecifier? InjectSound;
}

public sealed partial class UseInjectReagentsActionEvent : InstantActionEvent;