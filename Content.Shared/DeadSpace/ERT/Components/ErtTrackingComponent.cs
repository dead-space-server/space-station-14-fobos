// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Pinpointer;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.ERT.Components;

/// <summary>
/// Stores target tracking data for an ERT operative and mirrors pinpointer-style alert state to the client.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ErtTrackingComponent : Component
{
    [DataField]
    public float MediumDistance = 16f;

    [DataField]
    public float CloseDistance = 8f;

    [DataField]
    public float ReachedDistance = 1f;

    /// <summary>
    /// Arrow precision in radians.
    /// </summary>
    [DataField]
    public double Precision = 0.09;

    /// <summary>
    /// Server-side selected target.
    /// </summary>
    [ViewVariables]
    public EntityUid? Target;

    [ViewVariables, AutoNetworkedField]
    public Angle ArrowAngle;

    [ViewVariables, AutoNetworkedField]
    public Distance DistanceToTarget = Distance.Unknown;

    [ViewVariables, AutoNetworkedField]
    public string? TargetName;

    [ViewVariables, AutoNetworkedField]
    public string? TargetJobName;
}
