// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Inventory.Events;
using Content.Shared.StationAi;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Tag;

namespace Content.Shared.DeadSpace.StationAi;

public sealed class BodyCameraVisionSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    private const string BodyCameraTag = "BodyCamera";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiVisionComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<StationAiVisionComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, StationAiVisionComponent component, GotEquippedEvent args)
    {
        if (!_tag.HasTag(uid, BodyCameraTag))
            return;

        var tracker = EnsureComp<BodyCameraVisionComponent>(args.Equipee);
        tracker.SourceCount++;

        var vision = EnsureComp<StationAiVisionComponent>(args.Equipee);
        var stationAi = EntityManager.System<SharedStationAiSystem>();
        stationAi.SetVisionRange((args.Equipee, vision), tracker.SourceCount == 1 ? component.Range : Math.Max(vision.Range, component.Range));
        stationAi.SetVisionOccluded((args.Equipee, vision), component.Occluded);
        stationAi.SetVisionEnabled((args.Equipee, vision), true);
    }

    private void OnUnequipped(EntityUid uid, StationAiVisionComponent component, GotUnequippedEvent args)
    {
        if (!_tag.HasTag(uid, BodyCameraTag))
            return;

        if (!TryComp<BodyCameraVisionComponent>(args.Equipee, out var tracker))
            return;

        tracker.SourceCount--;

        if (tracker.SourceCount <= 0)
        {
            RemComp<BodyCameraVisionComponent>(args.Equipee);
            RemComp<StationAiVisionComponent>(args.Equipee);
        }
    }
}
