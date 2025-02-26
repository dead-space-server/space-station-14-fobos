using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.SmokingCarp.Abilities.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSmokingCarpNotRangeSystem))]
public sealed partial class SmokingCarpNotRangeComponent : Component { }
