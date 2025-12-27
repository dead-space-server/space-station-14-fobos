// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Virus.Components;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class CauseVirus : EventEntityEffect<CauseVirus>
{
    [DataField]
    public VirusData Data = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cause-virus", ("chance", Probability));
}
