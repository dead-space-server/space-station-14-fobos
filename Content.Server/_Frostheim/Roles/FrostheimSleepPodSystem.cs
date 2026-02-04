using Content.Shared._Frostheim.Roles;
using Robust.Shared.Containers;

namespace Content.Server._Frostheim.Roles;

public sealed class FrostheimSleepPodSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostheimSleepPodComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<FrostheimSleepPodComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(EntityUid uid, FrostheimSleepPodComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == comp.ContainerId)
            _appearance.SetData(uid, FrostheimSleepPodVisuals.Occupied, true);
    }

    private void OnRemoved(EntityUid uid, FrostheimSleepPodComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == comp.ContainerId)
            _appearance.SetData(uid, FrostheimSleepPodVisuals.Occupied, false);
    }
}
