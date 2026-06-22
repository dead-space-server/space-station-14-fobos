using Content.Shared.DeadSpace.Triggers.Components;
using Content.Shared.Trigger;
using Content.Shared.DeadSpace.CodeLock;

namespace Content.Shared.DeadSpace.Triggers.Systems;

public sealed class TriggerOnCodeLockStatusSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnCodeLockStatusComponent, CodeLockStatusChangedEvent>(OnCodeLockStatusChanged);
    }

    private void OnCodeLockStatusChanged(Entity<TriggerOnCodeLockStatusComponent> ent, ref CodeLockStatusChangedEvent args)
    {
        if (args.Status == ent.Comp.Status)
        {
            Trigger.Trigger(ent.Owner, null, ent.Comp.KeyOut);
        }
    }
}
