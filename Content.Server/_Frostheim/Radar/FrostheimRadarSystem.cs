using System.Numerics;
using Content.Shared._Frostheim.Radar;
using Content.Shared._Frostheim.Shuttle;
using Robust.Server.GameObjects;

namespace Content.Server._Frostheim.Radar;

public sealed class FrostheimRadarSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<FrostheimRadarComponent>(FrostheimRadarUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<BoundUIClosedEvent>(OnClosed);
        });
    }

    private void OnOpened(Entity<FrostheimRadarComponent> ent, ref BoundUIOpenedEvent args)
    {
        ent.Comp.ActiveUser = args.Actor;
        EnsureComp<ActiveFrostheimRadarComponent>(ent.Owner);
        SendUpdate(ent, ent.Comp);
    }

    private void OnClosed(Entity<FrostheimRadarComponent> ent, ref BoundUIClosedEvent args)
    {
        ent.Comp.ActiveUser = null;

        if (!_ui.IsUiOpen(ent.Owner, FrostheimRadarUiKey.Key))
            RemCompDeferred<ActiveFrostheimRadarComponent>(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveFrostheimRadarComponent, FrostheimRadarComponent>();
        while (query.MoveNext(out var uid, out var active, out var radar))
        {
            active.AccumulatedTime += frameTime;

            if (active.AccumulatedTime < radar.UpdateInterval)
                continue;

            active.AccumulatedTime -= radar.UpdateInterval;

            SendUpdate(uid, radar);
        }
    }

    private void SendUpdate(EntityUid uid, FrostheimRadarComponent radar)
    {
        if (radar.ActiveUser is not { } player)
            return;

        if (!TryComp<TransformComponent>(player, out var playerXform))
            return;

        var playerPos = _xform.GetWorldPosition(playerXform);
        var playerMap = playerXform.MapID;

        var blips = new List<RadarBlip>();
        var totalThrusters = 0;
        var totalGyroscopes = 0;

        var partQuery = EntityQueryEnumerator<FrostheimBrokenPartComponent, TransformComponent>();
        while (partQuery.MoveNext(out _, out var part, out var partXform))
        {
            if (partXform.MapID != playerMap)
                continue;

            if (part.PartType == FrostheimPartType.Thruster)
                totalThrusters++;
            else
                totalGyroscopes++;

            var partPos = _xform.GetWorldPosition(partXform);
            var delta = partPos - playerPos;
            var distance = delta.Length();
            var angle = MathF.Atan2(delta.Y, delta.X);

            blips.Add(new RadarBlip(part.PartType, angle, distance));
        }

        _ui.ServerSendUiMessage(uid, FrostheimRadarUiKey.Key,
            new FrostheimRadarUpdateMessage(blips, totalThrusters, totalGyroscopes, radar.MaxRange));
    }
}
