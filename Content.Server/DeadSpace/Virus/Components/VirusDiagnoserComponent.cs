// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.DeadSpace.Virus;
using Content.Shared.FixedPoint;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusDiagnoserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedConsole = null;

    /// <summary>
    ///     Длительность анимации печати отчёта. Костыль, но упрощает систему.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow AnimationWindow = new TimedWindow(TimeSpan.FromSeconds(4f), TimeSpan.FromSeconds(4f));

    /// <summary>
    ///     Данные которые печатаются в отчёт или генерируются в реагент.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusData? VirusDataCPU = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public BloodVirusAnalysisResult? BloodAnalysisResult = default!;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<SinkPortPrototype> VirusDiagnoserPort = "VirusDiagnoserReceiver";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Paper = "DiagnosisReportPaper";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusDiagnoserStatus Status = VirusDiagnoserStatus.Off;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? PrintingSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? ScanningSound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/scanning.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? DenialSound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/denial.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? SuccessfullySound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/success.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? GenerateVirusSound = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CurrentSoundEntity = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextConsoleStatusUpdate = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ScanStartedAt = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ScanDuration = TimeSpan.Zero;
}

public sealed class BloodVirusAnalysisResult
{
    public string PatientName = string.Empty;
    public string PatientDna = string.Empty;
    public string BloodTypes = string.Empty;
    public FixedPoint2 BloodVolume = FixedPoint2.Zero;
    public VirusData? VirusData;
    public string? KnownStrainName;
    public string DiseaseStatus = string.Empty;
}
