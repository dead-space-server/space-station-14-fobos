// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Instruments;

[RegisterComponent]
public sealed partial class HeadphonesInstrumentComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId NoiseCancellingAction = "ToggleNoiseCancellingAction";
}

[RegisterComponent]
public sealed partial class NoiseCancellingWearerComponent : Component
{
    public readonly HashSet<EntityUid> Headphones = [];
}
