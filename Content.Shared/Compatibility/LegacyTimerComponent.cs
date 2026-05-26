using Robust.Shared.GameStates;

namespace Content.Shared.Compatibility;

// DS14-start: compatibility for legacy maps/prototypes serialized before Robust removed TimerComponent.
[RegisterComponent, NetworkedComponent]
public sealed partial class TimerComponent : Component;
// DS14-end
