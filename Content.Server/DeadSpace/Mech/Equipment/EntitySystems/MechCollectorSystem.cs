using Content.Server.Mech.Equipment.Components;
using Content.Server.Mech.Systems;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Server.Mech.Equipment.EntitySystems;

public sealed class MechCollectorSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MechSystem _mech = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MechCollectorComponent, MechEquipmentComponent, StorageComponent>();

        while (query.MoveNext(out var uid, out var comp, out var equip, out var storage))
        {
            if (comp.NextScan == TimeSpan.Zero)
                comp.NextScan = now + comp.ScanInterval;

            if (comp.NextScan > now)
                continue;

            comp.NextScan += comp.ScanInterval;

            var collectorXform = Transform(uid);
            if (collectorXform.ParentUid != equip.EquipmentOwner)
                continue;

            if (equip.EquipmentOwner is not { } mech)
                continue;

            if (!TryComp<MechComponent>(mech, out var mechComp))
                continue;

            if (mechComp.Broken)
                continue;

            TryCollect(uid, mech, comp, storage);
        }
    }

    private void TryCollect(EntityUid uid, EntityUid mech, MechCollectorComponent comp, StorageComponent storage)
    {
        if (!_storage.HasSpace((uid, storage)))
            return;

        var collectorXform = Transform(uid);
        var finalCoords = collectorXform.Coordinates;

        var collectedAny = false;

        foreach (var ent in _lookup.GetEntitiesInRange(uid, comp.Range, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (!_storage.HasSpace((uid, storage)))
                break;

            if (ent == uid || ent == mech)
                continue;

            if (!_physicsQuery.TryGetComponent(ent, out var phys) || phys.BodyStatus != BodyStatus.OnGround)
                continue;

            if (!_whitelist.IsWhitelistPassOrNull(comp.Whitelist, ent))
                continue;

            if (!_mech.TryChangeEnergy(mech, comp.CollectEnergyDelta))
                continue;

            var nearXform = Transform(ent);
            var nearMap = _transform.GetMapCoordinates(ent, nearXform);
            var moverCoords = _transform.GetMoverCoordinates(uid, collectorXform);
            var nearCoords = _transform.ToCoordinates(moverCoords.EntityId, nearMap);

            if (!_storage.Insert(uid, ent, out var stacked, storageComp: storage, playSound: false))
                continue;

            collectedAny = true;

            if (stacked != null)
                _storage.PlayPickupAnimation(stacked.Value, nearCoords, finalCoords, nearXform.LocalRotation);
            else
                _storage.PlayPickupAnimation(ent, nearCoords, finalCoords, nearXform.LocalRotation);
        }

        if (collectedAny)
            _audio.PlayPvs(comp.Sound, mech);
    }
}
