using Robust.Shared.GameStates;

namespace Content.Shared.Sound.Components;

/// <summary>
/// Simple sound emitter that emits sound on entity pickup
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmitSoundOnPickupComponent : BaseEmitSoundComponent;

// DS14-start
[RegisterComponent]
public sealed partial class SuppressPickupDropSoundComponent : Component;
// DS14-end
