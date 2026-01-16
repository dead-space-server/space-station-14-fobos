using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.MartialArts.SmokingCarp.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedMartialArtsSystem))]
public sealed partial class SmokingCarpNotShotComponent : Component { }
