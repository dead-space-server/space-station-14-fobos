using Robust.Shared.GameStates;

namespace Content.Shared.Tram.Components;

/// <summary>
/// Syndicate device that allows remote control of the tram.
/// Enables overdrive mode which disables safety protocols.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTramSystem))]
public sealed partial class TramRemoteControlComponent : Component
{
    /// <summary>
    /// Whether overdrive mode is currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool OverdriveEnabled;

    /// <summary>
    /// The tram car this remote is linked to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedTram;
}
