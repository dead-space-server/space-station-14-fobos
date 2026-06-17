using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.DeadSpace.Borgs;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;

namespace Content.Server.DeadSpace.Borgs;

public sealed class BorgRideSystem : EntitySystem
{
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtual = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgRideComponent, StrapAttemptEvent>(OnBorgStrapAttempt);
        SubscribeLocalEvent<BorgRideComponent, StrappedEvent>(OnBorgStrapped);
        SubscribeLocalEvent<BorgRideComponent, UnstrappedEvent>(OnBorgUnstrapped);
        SubscribeLocalEvent<BorgRideComponent, VirtualItemDeletedEvent>(OnBorgVirtualItemDeleted);
        SubscribeLocalEvent<BorgRiderComponent, PickupAttemptEvent>(OnRiderPickupAttempt);
        SubscribeLocalEvent<BorgRiderComponent, AttackAttemptEvent>(OnRiderAttackAttempt);
    }

    private void OnBorgStrapAttempt(Entity<BorgRideComponent> ent, ref StrapAttemptEvent args)
    {
        if (!HasComp<HandsComponent>(args.Buckle.Owner))
            args.Cancelled = true;
    }

    private void OnBorgStrapped(Entity<BorgRideComponent> ent, ref StrappedEvent args)
    {
        var rider = args.Buckle.Owner;

        if (!HasComp<HandsComponent>(rider))
        {
            _buckle.TryUnbuckle(rider, rider);
            return;
        }

        foreach (var hand in _hands.EnumerateHands(rider))
        {
            if (_hands.TryGetHeldItem(rider, hand, out _))
                _hands.TryDrop(rider, hand, checkActionBlocker: false);
        }

        if (!_virtual.TrySpawnVirtualItemInHand(ent.Owner, rider, out _))
        {
            _buckle.TryUnbuckle(rider, rider);
            return;
        }

        if (!_virtual.TrySpawnVirtualItemInHand(ent.Owner, rider, out _))
        {
            _virtual.DeleteInHandsMatching(rider, ent.Owner);
            _buckle.TryUnbuckle(rider, rider);
            return;
        }

        EnsureComp<BorgRiderComponent>(rider);
    }

    private void OnBorgUnstrapped(Entity<BorgRideComponent> ent, ref UnstrappedEvent args)
    {
        var rider = args.Buckle.Owner;

        _virtual.DeleteInHandsMatching(rider, ent.Owner);

        RemComp<BorgRiderComponent>(rider);
    }

    private void OnBorgVirtualItemDeleted(Entity<BorgRideComponent> ent, ref VirtualItemDeletedEvent args)
    {
        if (!TryComp<BuckleComponent>(args.User, out var buckle) || buckle.BuckledTo != ent.Owner)
            return;

        _buckle.TryUnbuckle(args.User, args.User, buckle);
    }

    private void OnRiderPickupAttempt(Entity<BorgRiderComponent> ent, ref PickupAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRiderAttackAttempt(Entity<BorgRiderComponent> ent, ref AttackAttemptEvent args)
    {
        args.Cancel();
    }
}
