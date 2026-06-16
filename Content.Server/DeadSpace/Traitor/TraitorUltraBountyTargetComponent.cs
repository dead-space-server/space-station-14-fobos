// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.FixedPoint;

namespace Content.Server.DeadSpace.Traitor;

[RegisterComponent, Access(typeof(TraitorUltraRuleSystem))]
public sealed partial class TraitorUltraBountyTargetComponent : Component
{
    public EntityUid Rule;
    public EntityUid MindId;
    public readonly Dictionary<EntityUid, FixedPoint2> DamageByMind = new();
    public readonly Dictionary<EntityUid, EntityUid> DamageSourceByMind = new();
}
