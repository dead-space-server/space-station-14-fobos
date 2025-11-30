// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.Virus;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusDiagnoserDataServerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedConsole = null;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<SinkPortPrototype> VirusDiagnoserDataServerPort = "VirusDiagnoserDataServerReceiver";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<VirusStrainRecord, VirusData> StrainData = new();

    /// <summary>
    ///     Исследовательские очки.
    /// </summary>
    [DataField]
    public int Points = 0;
}
