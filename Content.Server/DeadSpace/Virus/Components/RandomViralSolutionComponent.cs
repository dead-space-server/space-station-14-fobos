// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Server.DeadSpace.Virus.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent, Access(typeof(RandomViralSolutionSystem))]
public sealed partial class RandomViralSolutionComponent : Component
{
    [DataField]
    public string Solution = "drink";

    [DataField]
    public ProtoId<ReagentPrototype> Reagent = "ViralSolution";

    [DataField]
    public FixedPoint2 Quantity = 30;

    [DataField]
    public int MinSymptoms = 1;

    [DataField]
    public int MaxSymptoms = 3;

    [DataField]
    public List<DangerIndicatorSymptom> AllowedDangers = new() { DangerIndicatorSymptom.Low };

    [DataField]
    public List<DangerIndicatorSymptom> RequiredDangers = new();

    [DataField]
    public List<ProtoId<BodyPrototype>> BodyWhitelist = new() { "Human" };
}
