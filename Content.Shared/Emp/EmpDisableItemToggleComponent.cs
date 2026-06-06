using Robust.Shared.GameStates;

namespace Content.Shared.Emp;

[RegisterComponent, NetworkedComponent]
public sealed partial class EmpDisableItemToggleComponent : Component;

// DS14-start EMP-disables-toggle
[ByRefEvent]
public readonly record struct EmpItemToggleDisabledEvent(EntityUid? User);
// DS14-end
