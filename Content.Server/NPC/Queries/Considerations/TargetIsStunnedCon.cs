using System.Collections.Generic;
using Robust.Shared.Prototypes;

namespace Content.Server.NPC.Queries.Considerations;

/// <summary>
/// Returns 1f if the target has the <see cref="StunnedComponent"/>
/// </summary>
public sealed partial class TargetIsStunnedCon : UtilityConsideration
{
    // DS14-start
    [DataField("ownerBatteryAmmoPrototypes")]
    public HashSet<EntProtoId>? OwnerBatteryAmmoPrototypes;
    // DS14-end
}

