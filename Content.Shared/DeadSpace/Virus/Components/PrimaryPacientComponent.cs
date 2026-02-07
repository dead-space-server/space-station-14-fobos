// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Virus.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PrimaryPacientComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string StrainId;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SentientVirus = default!;

    /// <summary>
    ///     Радиус распространения вируса.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float RangeInfect = 2f;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow UpdateWindow = new TimedWindow(TimeSpan.FromSeconds(1f), TimeSpan.FromSeconds(5f));

    public PrimaryPacientComponent(EntityUid sentientVirus, string strainId)
    {
        StrainId = strainId;
        SentientVirus = sentientVirus;
    }

    public PrimaryPacientComponent(string strainId)
    {
        StrainId = strainId;
    }

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "PrimaryPacientFaction";
}

