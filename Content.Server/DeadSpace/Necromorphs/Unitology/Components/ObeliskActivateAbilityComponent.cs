// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Necromorphs.Unitology.Components;

[RegisterComponent]
public sealed partial class ObeliskActivateAbilityComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId ActionObeliskActivate = "ActionObeliskActivate";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionObeliskActivateEntity;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ActivationDuration = TimeSpan.FromSeconds(10);

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float ActivationRange = 5f;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float ActivationDistantion = 2f;
}
