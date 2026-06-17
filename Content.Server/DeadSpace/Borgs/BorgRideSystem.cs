using Content.Shared.Buckle.Components;
using Content.Shared.DeadSpace.Borgs;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;

namespace Content.Server.DeadSpace.Borgs;

public sealed class BorgRideSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtual = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgRideComponent, StrappedEvent>(OnBorgStrapped);
        SubscribeLocalEvent<BorgRideComponent, UnstrappedEvent>(OnBorgUnstrapped);
        SubscribeLocalEvent<BorgRiderComponent, PickupAttemptEvent>(OnRiderPickupAttempt);
        SubscribeLocalEvent<BorgRiderComponent, AttackAttemptEvent>(OnRiderAttackAttempt);
    }

    private void OnBorgStrapped(Entity<BorgRideComponent> ent, ref StrappedEvent args)
    {
        var rider = args.Buckle.Owner;

        if (!HasComp<HandsComponent>(rider))
            return;

        foreach (var hand in _hands.EnumerateHands(rider))
        {
            if (_hands.TryGetHeldItem(rider, hand, out _))
                _hands.TryDrop(rider, hand, checkActionBlocker: false);
        }

        if (!_virtual.TrySpawnVirtualItemInHand(ent.Owner, rider, out _))
            return;

        if (!_virtual.TrySpawnVirtualItemInHand(ent.Owner, rider, out _))
        {
            _virtual.DeleteInHandsMatching(rider, ent.Owner);
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

    private void OnRiderPickupAttempt(Entity<BorgRiderComponent> ent, ref PickupAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRiderAttackAttempt(Entity<BorgRiderComponent> ent, ref AttackAttemptEvent args)
    {
        args.Cancel();
    }
}
