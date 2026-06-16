using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Triggers.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WhistleOnTriggerComponent : BaseXOnTriggerComponent;