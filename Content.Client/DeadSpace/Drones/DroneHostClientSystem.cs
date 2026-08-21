using Content.Shared.DeadSpace.Drones.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.DeadSpace.Drones;

public sealed class DroneHostClientSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DroneHostComponent, GetStatusIconsEvent>(OnGetStatusIcon);
    }

    private void OnGetStatusIcon(EntityUid uid, DroneHostComponent component, ref GetStatusIconsEvent args)
    {
        args.StatusIcons.Add(_prototype.Index(component.Icon));
    }
}