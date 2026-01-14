// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeviceLinking;
using Content.Shared.DeadSpace.Virus;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusSolutionAnalyzerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedConsole = null;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ConnectedEvolutionConsole = null;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<SinkPortPrototype> VirusSolutionAnalyzerPort = "VirusSolutionAnalyzerReceiver";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? PrintingSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusSolutionAnalyzerStatus Status = VirusSolutionAnalyzerStatus.Off;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? ScanningSound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/scanning.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? DenialSound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/denial.ogg");

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? SuccessfullySound = new SoundPathSpecifier("/Audio/_DeadSpace/Virus/Diagnoser/success.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CurrentSoundEntity = default!;
}
