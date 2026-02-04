using Content.Shared.Popups;
using Content.Shared.UserInterface;

namespace Content.Shared._Frostheim.Shuttle;

public sealed class SharedFrostheimShuttleSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostheimShuttleConsoleComponent, ActivatableUIOpenAttemptEvent>(OnShuttleConsoleOpenAttempt);
        SubscribeLocalEvent<FrostheimNavConsoleComponent, ActivatableUIOpenAttemptEvent>(OnNavOpenAttempt);
    }

    private void OnShuttleConsoleOpenAttempt(EntityUid uid, FrostheimShuttleConsoleComponent comp, ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var gridUid = Transform(uid).GridUid;
        if (gridUid == null || !TryComp<FrostheimShuttleComponent>(gridUid, out var shuttle))
            return;

        if (!shuttle.Ready)
        {
            args.Cancel();
            _popup.PopupCursor(Loc.GetString("frostheim-shuttle-not-ready"), args.User);
        }
    }

    private void OnNavOpenAttempt(EntityUid uid, FrostheimNavConsoleComponent comp, ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (comp.NavigationComplete)
        {
            args.Cancel();
            _popup.PopupClient(Loc.GetString("frostheim-nav-already-complete"), uid, args.User);
            return;
        }

        var gridUid = Transform(uid).GridUid;
        if (gridUid == null || !TryComp<FrostheimShuttleComponent>(gridUid, out var shuttle))
            return;

        if (!shuttle.HasGyroscope)
        {
            args.Cancel();
            _popup.PopupClient(Loc.GetString("frostheim-nav-no-gyroscope"), uid, args.User);
        }
    }
}
