// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.Whitelist;
using Content.Shared.Humanoid.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusDiagnoserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedConsole = null;

    /// <summary>
    ///     Длительность анимации печати отчёта. Костыль, но упрощает систему.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float PrintingAnimationTime = 5f;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<SinkPortPrototype> VirusDiagnoserPort = "VirusDiagnoserReceiver";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityId Paper = "DrugInitializeDiagnoserReportPaper";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusDiagnoserStatus Status = VirusDiagnoserStatus.Off;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusDiagnoserVisuals Visual = VirusDiagnoserVisuals.Status;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? PrintingSound = default!;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? ScanningSound = default!;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? DenielSound = default!;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? SuccessfullySound = default!;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? GenerateVirusSound = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? GenerateVirusSoundEntity = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? PrintingSoundEntity = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ScanningSoundEntity = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? DenielSoundEntity = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SuccessfullySoundEntity = default!;
}
