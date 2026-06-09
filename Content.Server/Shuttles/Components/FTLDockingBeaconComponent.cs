using Content.Shared.Whitelist;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Makes an FTL beacon route console jumps to dock with a target grid when it exists.
/// If the target grid is gone, the beacon still works as a regular coordinate fallback.
/// </summary>
[RegisterComponent]
public sealed partial class FTLDockingBeaconComponent : Component
{
    [ViewVariables]
    public EntityUid? TargetGrid;

    [ViewVariables(VVAccess.ReadWrite)]
    public float? StartupTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public float? HyperspaceTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public string? PriorityTag;

    /// <summary>
    /// Shuttles allowed to use the docking route. Others still use the beacon as a coordinate fallback.
    /// </summary>
    [DataField]
    public EntityWhitelist? DockWhitelist;

    [DataField]
    public float FallbackMinOffset = 8f;

    [DataField]
    public float FallbackMaxOffset = 96f;
}
