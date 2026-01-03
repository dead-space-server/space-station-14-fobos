// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeadSpace.Virus;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusDiagnoserDataServerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedConsole = null;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedEvolutionConsole = null;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<SinkPortPrototype> VirusDiagnoserDataServerPort = "VirusDiagnoserDataServerReceiver";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<VirusStrainRecord, VirusData> StrainData = new();

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Disk = "ResearchDisk";

    /// <summary>
    ///     Длительность обновления данных.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow UpdateWindow = new TimedWindow(TimeSpan.FromSeconds(1f), TimeSpan.FromSeconds(1f));

    /// <summary>
    ///     Множитель получаемых очков за каждый хранимый симптом.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int SymptomsPointsMultiply = 2;

    /// <summary>
    ///     Множитель получаемых очков за каждое тело.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int BodyPointsMultiply = 1;

    /// <summary>
    ///     Исследовательские очки.
    /// </summary>
    [DataField]
    public int Points = 0;
}
