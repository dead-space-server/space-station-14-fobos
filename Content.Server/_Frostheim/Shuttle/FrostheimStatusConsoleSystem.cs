using Content.Shared._Frostheim.Shuttle;
using Robust.Server.GameObjects;

namespace Content.Server._Frostheim.Shuttle;

public sealed class FrostheimStatusConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<FrostheimStatusConsoleComponent>(FrostheimStatusConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUIOpened);
        });

        SubscribeLocalEvent<FrostheimShuttleComponent, ComponentStartup>(OnShuttleChanged);
    }

    private void OnShuttleChanged(EntityUid uid, FrostheimShuttleComponent comp, ComponentStartup args)
    {
    }

    private void OnUIOpened(EntityUid uid, FrostheimStatusConsoleComponent comp, BoundUIOpenedEvent args)
    {
        SendState(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FrostheimStatusConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (!_ui.IsUiOpen(uid, FrostheimStatusConsoleUiKey.Key))
                continue;

            SendState(uid);
        }
    }

    private void SendState(EntityUid uid)
    {
        var gridUid = Transform(uid).GridUid;
        if (gridUid == null || !TryComp<FrostheimShuttleComponent>(gridUid, out var shuttle))
            return;

        var state = new FrostheimStatusConsoleState
        {
            ThrustersNeeded = shuttle.InitialThrusters + shuttle.AdditionalThrustersNeeded,
            ThrustersCurrent = shuttle.CurrentThrusters,
            GyroscopeRequired = shuttle.RequireGyroscope,
            GyroscopeInstalled = shuttle.HasGyroscope,
            NavigationComplete = shuttle.NavigationComplete,
            Ready = shuttle.Ready,
        };

        _ui.SetUiState(uid, FrostheimStatusConsoleUiKey.Key, state);
    }
}
