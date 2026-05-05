// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Physics.Events;

namespace Content.Shared.DeadSpace.EntityFilterBarrier;

[RegisterComponent, NetworkedComponent]
public sealed partial class EntityFilterBarrierComponent : Component
{
    [DataField("blockedPrototypes")]
    public List<string> BlockedPrototypes = new();
}

public abstract partial class SharedEntityFilterBarrierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntityFilterBarrierComponent, PreventCollideEvent>(OnPreventCollide);
    }

    protected virtual void OnPreventCollide(EntityUid uid, EntityFilterBarrierComponent component, ref PreventCollideEvent args)
    {
        var protoId = MetaData(args.OtherEntity).EntityPrototype?.ID;

        if (protoId == null || !component.BlockedPrototypes.Contains(protoId))
            args.Cancelled = true;
    }
}