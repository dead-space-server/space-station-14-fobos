/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Shared.Vehicle.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Shared.Vehicle;

public sealed partial class VehicleSystem
{
    private void InitializeKey()
    {
        SubscribeLocalEvent<GenericKeyedVehicleComponent, ContainerIsInsertingAttemptEvent>(OnGenericKeyedInsertAttempt);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, EntInsertedIntoContainerMessage>(OnGenericKeyedEntInserted);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, EntRemovedFromContainerMessage>(OnGenericKeyedEntRemoved);
        SubscribeLocalEvent<GenericKeyedVehicleComponent, VehicleCanRunEvent>(OnGenericKeyedCanRun);
    }

    private void OnGenericKeyedInsertAttempt(Entity<GenericKeyedVehicleComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Cancelled || _timing.ApplyingState || !ent.Comp.PreventInvalidInsertion || args.Container.ID != ent.Comp.ContainerId)
            return;

        // DS14-start
        if (TryComp<VehicleKeyComponent>(args.EntityUid, out var keyComp) &&
            keyComp.BoundVehicle is { } boundVehicle &&
            boundVehicle != ent.Owner)
        {
            var keyHolder = Transform(args.EntityUid).ParentUid;
            if (keyHolder.IsValid() && _timing.CurTime >= ent.Comp.NextWrongKeyPopup)
            {
                ent.Comp.NextWrongKeyPopup = _timing.CurTime + TimeSpan.FromSeconds(3);
                _popup.PopupEntity(
                    Loc.GetString("vehicle-key-wrong"),
                    args.EntityUid,
                    keyHolder,
                    PopupType.SmallCaution
                );
            }
            args.Cancel();
            return;
        }
        // DS14-end

        if (ent.Comp.BoundKey is { } boundKey)
        {
            if (args.EntityUid != boundKey)
            {
                var keyHolder = Transform(args.EntityUid).ParentUid;
                if (keyHolder.IsValid() && _timing.CurTime >= ent.Comp.NextWrongKeyPopup)
                {
                    ent.Comp.NextWrongKeyPopup = _timing.CurTime + TimeSpan.FromSeconds(3);
                    _popup.PopupEntity(
                        Loc.GetString("vehicle-key-wrong"),
                        args.EntityUid,
                        keyHolder,
                        PopupType.SmallCaution
                    );
                }
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

        if (ent.Comp.BoundKey is null &&
            _entityWhitelist.IsWhitelistPass(ent.Comp.KeyWhitelist, args.Entity))
        {
            ent.Comp.BoundKey = args.Entity;
            Dirty(ent);

            //DS14-start
            var keyComp = EnsureComp<VehicleKeyComponent>(args.Entity);
            keyComp.BoundVehicle = ent.Owner;
            Dirty(args.Entity, keyComp);
            //DS14-end
        }

        if (!_vehicleQuery.TryComp(ent, out var vehicle))
            return;

        RefreshCanRun((ent.Owner, vehicle));

        //DS14-start
        if (vehicle.Operator is { } operatorUid)
            _actionBlocker.UpdateCanMove(operatorUid);
        //DS14-end
    }

    private void OnGenericKeyedEntRemoved(Entity<GenericKeyedVehicleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState || args.Container.ID != ent.Comp.ContainerId)
            return;

        if (!_vehicleQuery.TryComp(ent, out var vehicle))
            return;

        RefreshCanRun((ent.Owner, vehicle));

        //DS14-start
        if (vehicle.Operator is { } operatorUid)
            _actionBlocker.UpdateCanMove(operatorUid);
        //DS14-end
    }

    private void OnGenericKeyedCanRun(Entity<GenericKeyedVehicleComponent> ent, ref VehicleCanRunEvent args)
    {
        if (!args.CanRun)
            return;

        if (!_container.TryGetContainer(ent.Owner, ent.Comp.ContainerId, out var container))
        {
            args = args with { CanRun = false };
            return;
        }

        var hasKey = container.ContainedEntities.Any(contained =>
            !_entityWhitelist.IsWhitelistFail(ent.Comp.KeyWhitelist, contained));

        if (!hasKey)
            args = args with { CanRun = false };
    }
}