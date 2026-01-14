using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Server.DeadSpace.MartialArts.Components;

namespace Content.Server.DeadSpace.MartialArts;

public abstract partial class ServerMartialArtsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SmokingCarpComponent, ShotAttemptedEvent>(OnShotAttempt);
    }

    private void OnShotAttempt(Entity<SmokingCarpComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.MartialArtsForm != MartialArtsForms.SmokingCarp)
            return;
        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}
