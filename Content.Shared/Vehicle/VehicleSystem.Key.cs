/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Vehicle.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Shared.Vehicle;

public sealed partial class VehicleSystem
{
    private void InitializeKey()
    {
        SubscribeLocalEvent<GenericKeyedVehicleComponent, ComponentShutdown>(OnGenericKeyedShutdown);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, ContainerIsInsertingAttemptEvent>(OnGenericKeyedInsertAttempt);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, EntInsertedIntoContainerMessage>(OnGenericKeyedEntInserted);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, EntRemovedFromContainerMessage>(OnGenericKeyedEntRemoved);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, ItemSlotEjectAttemptEvent>(OnGenericKeyedEjectAttempt); // DS14
        SubscribeLocalEvent<GenericKeyedVehicleComponent, VehicleCanRunEvent>(OnGenericKeyedCanRun);

        SubscribeLocalEvent<VehicleKeyComponent, ComponentShutdown>(OnVehicleKeyShutdown);
    }

    private void OnGenericKeyedInsertAttempt(Entity<GenericKeyedVehicleComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Cancelled || _timing.ApplyingState || !ent.Comp.PreventInvalidInsertion || args.Container.ID != ent.Comp.ContainerId)
            return;

        ClearDeletedBoundKey(ent);

        if (TryComp<VehicleKeyComponent>(args.EntityUid, out var keyComp) &&
            keyComp.BoundVehicle is { } boundVehicle &&
            boundVehicle != ent.Owner)
        {
            if (Deleted(boundVehicle))
            {
                keyComp.BoundVehicle = null;
                Dirty(args.EntityUid, keyComp);
            }
            else
            {
                PopupWrongKey(args.EntityUid, ent.Comp);
                args.Cancel();
                return;
            }
        }

        if (ent.Comp.BoundKey is { } boundKey)
        {
            if (args.EntityUid != boundKey)
            {
                PopupWrongKey(args.EntityUid, ent.Comp);
                args.Cancel();
            }
            return;
        }

        if (_entityWhitelist.IsWhitelistPass(ent.Comp.KeyWhitelist, args.EntityUid))
            return;

        args.Cancel();
    }

    private void OnGenericKeyedEntInserted(Entity<GenericKeyedVehicleComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState || args.Container.ID != ent.Comp.ContainerId)
            return;

        ClearDeletedBoundKey(ent);

        if (ent.Comp.BoundKey is null || ent.Comp.BoundKey == args.Entity)
            TryBindKey(ent, args.Entity);

        RefreshKeyedVehicle(ent);
    }

    // DS14-start
    private void OnGenericKeyedEjectAttempt(Entity<GenericKeyedVehicleComponent> ent, ref ItemSlotEjectAttemptEvent args)
    {
        if (args.Cancelled || args.Slot.ContainerSlot?.ID != ent.Comp.ContainerId)
            return;

        if (!_vehicleQuery.TryComp(ent, out var vehicle) || !vehicle.KeyEjectRequiresOperator)
            return;

        if (args.User == null || args.User == vehicle.Operator)
            return;

        args.Cancelled = true;
        _popup.PopupPredicted(
            Loc.GetString("vehicle-key-eject-requires-operator"),
            ent.Owner,
            args.User,
            PopupType.SmallCaution
        );
    }
    // DS14-end

    private void OnGenericKeyedEntRemoved(Entity<GenericKeyedVehicleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState || args.Container.ID != ent.Comp.ContainerId)
            return;

        RefreshKeyedVehicle(ent);
    }

    private void OnGenericKeyedShutdown(Entity<GenericKeyedVehicleComponent> ent, ref ComponentShutdown args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.BoundKey is not { } boundKey)
            return;

        if (!TryComp<VehicleKeyComponent>(boundKey, out var key) || key.BoundVehicle != ent.Owner)
            return;

        key.BoundVehicle = null;
        Dirty(boundKey, key);
    }

    private void OnVehicleKeyShutdown(Entity<VehicleKeyComponent> ent, ref ComponentShutdown args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.BoundVehicle is not { } vehicleUid ||
            !TryComp<GenericKeyedVehicleComponent>(vehicleUid, out var keyed) ||
            keyed.BoundKey != ent.Owner)
        {
            return;
        }

        keyed.BoundKey = null;
        Dirty(vehicleUid, keyed);
        RefreshKeyedVehicle((vehicleUid, keyed));
    }

    private void OnGenericKeyedCanRun(Entity<GenericKeyedVehicleComponent> ent, ref VehicleCanRunEvent args)
    {
        if (!args.CanRun)
            return;

        if (IsMissingRequiredKey(ent))
            args = args with { CanRun = false };
    }

    private bool TryBindKey(Entity<GenericKeyedVehicleComponent> ent, EntityUid keyUid)
    {
        if (_entityWhitelist.IsWhitelistFail(ent.Comp.KeyWhitelist, keyUid))
            return false;

        var key = EnsureComp<VehicleKeyComponent>(keyUid);
        if (key.BoundVehicle is { } boundVehicle && boundVehicle != ent.Owner)
        {
            if (!Deleted(boundVehicle))
                return false;

            key.BoundVehicle = null;
        }

        ent.Comp.BoundKey = keyUid;
        key.BoundVehicle = ent.Owner;

        Dirty(ent);
        Dirty(keyUid, key);
        return true;
    }

    private void ClearDeletedBoundKey(Entity<GenericKeyedVehicleComponent> ent)
    {
        if (ent.Comp.BoundKey is not { } boundKey || !Deleted(boundKey))
            return;

        ent.Comp.BoundKey = null;
        Dirty(ent);
    }

    private void RefreshKeyedVehicle(Entity<GenericKeyedVehicleComponent> ent)
    {
        if (!_vehicleQuery.TryComp(ent.Owner, out var vehicle))
            return;

        RefreshCanRun((ent.Owner, vehicle));

        if (vehicle.Operator is { } operatorUid)
            _actionBlocker.UpdateCanMove(operatorUid);
    }

    private bool IsMissingRequiredKey(EntityUid vehicleUid)
    {
        return TryComp<GenericKeyedVehicleComponent>(vehicleUid, out var keyed) &&
            IsMissingRequiredKey((vehicleUid, keyed));
    }

    private bool IsMissingRequiredKey(Entity<GenericKeyedVehicleComponent> ent)
    {
        if (!_container.TryGetContainer(ent.Owner, ent.Comp.ContainerId, out var container))
            return true;

        ClearDeletedBoundKey(ent);

        if (ent.Comp.BoundKey is { } boundKey)
            return !container.ContainedEntities.Contains(boundKey);

        return !container.ContainedEntities.Any(contained =>
            !_entityWhitelist.IsWhitelistFail(ent.Comp.KeyWhitelist, contained));
    }

    private void PopupWrongKey(EntityUid keyUid, GenericKeyedVehicleComponent component)
    {
        var keyHolder = Transform(keyUid).ParentUid;
        if (!keyHolder.IsValid() || _timing.CurTime < component.NextWrongKeyPopup)
            return;

        component.NextWrongKeyPopup = _timing.CurTime + TimeSpan.FromSeconds(3);
        _popup.PopupPredicted(
            Loc.GetString("vehicle-key-wrong"),
            null,
            keyUid,
            keyHolder,
            PopupType.SmallCaution
        );
    }
}
