// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Traitor.Objectives;

[RegisterComponent, Access(typeof(TraitorUltraHijackTradeConditionSystem))]
public sealed partial class TraitorUltraHijackTradeConditionComponent : Component
{
    [DataField(required: true)]
    public LocId Title;

    [DataField(required: true)]
    public LocId Description;

    public EntityUid? TargetStation;
    public bool Completed;
}
