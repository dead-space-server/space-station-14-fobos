// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Weapons.Ranged.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Weapons.Ranged;

public sealed class SharedGunUsageBlockerSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> WizardWandTag = "WizardWand";

    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUsageBlockerComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<GunUsageBlockerComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_tag.HasTag(args.Used, WizardWandTag))
            return;

        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}
