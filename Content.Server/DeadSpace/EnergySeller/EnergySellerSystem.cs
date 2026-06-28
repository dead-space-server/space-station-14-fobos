using Content.Shared.DeadSpace.EnergySeller;
using Robust.Shared.Prototypes;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Power.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server.DeadSpace.EnergySeller;

public sealed partial class EnergySellerSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, BatteryStateChangedEvent>(CheckBatteryCharges);
        SubscribeLocalEvent<EnergySellerComponent, ChangesSpeedChargingForSellingEnergy>(SetSpeed);
    }
    private void CheckBatteryCharges(EntityUid uid, BatteryComponent comp, BatteryStateChangedEvent args)
    {
        if (comp.State != BatteryState.Full)
            return;
        if (!TryComp<EnergySellerComponent>(uid, out var compSell))
            return;

        var station = _station.GetOwningStation(uid);

        StationBankAccountComponent? bankAccount = null;
        if (station != null)
            TryComp(station.Value, out bankAccount);

        var bankAccountEnt = bankAccount!;
        var stationEnt = station!.Value;

        Dictionary<ProtoId<CargoAccountPrototype>, double> distribution = new Dictionary<ProtoId<CargoAccountPrototype>, double>
                        {
                            { compSell.Account, 1.0 },
                        };

        _cargo.UpdateBankAccount((stationEnt, bankAccountEnt), (int)Math.Round(comp.PricePerJoule * comp.MaxCharge + (comp.MaxCharge / compSell.AdditionalCoefficient + 1)), distribution, false);
        _battery.SetCharge(uid, 0);
        Dirty(stationEnt, bankAccountEnt);
    }
    private void SetSpeed(EntityUid uid, EnergySellerComponent comp, ChangesSpeedChargingForSellingEnergy message)
    {
        if (!TryComp<PowerNetworkBatteryComponent>(GetEntity(message.Entity), out var compSell))
            return;
        if (!(message.Now is null) || message.Now <= 5000)
            return;
        else
        {
            compSell.MaxChargeRate = Convert.ToSingle(message.Now);
        }
        if (message.Max == null || message.Max <= 5000)
            return;
        else
        {
            comp.MaxChargeRate = (int)message.Max;
        }
    }
    private void SetMaxLimit(EntityUid uid, EnergySellerComponent comp, ChangesSpeedChargingForSellingEnergy message)
    {
        if (!TryComp<BatteryComponent>(GetEntity(message.Entity), out var compSell))
            return;
        if (!(message.Now is null) || message.Now <= 5000)
            return;
        else
        {
            if (message.Now > compSell.LastCharge)
            {
                var station = _station.GetOwningStation(uid);

                StationBankAccountComponent? bankAccount = null;
                if (station != null)
                    TryComp(station.Value, out bankAccount);

                var bankAccountEnt = bankAccount!;
                var stationEnt = station!.Value;

                Dictionary<ProtoId<CargoAccountPrototype>, double> distribution = new Dictionary<ProtoId<CargoAccountPrototype>, double>
                                {
                                    { comp.Account, 1.0 },
                                };

                _cargo.UpdateBankAccount((stationEnt, bankAccountEnt), (int)Math.Round(compSell.PricePerJoule * compSell.MaxCharge + (compSell.MaxCharge / comp.AdditionalCoefficient + 1)), distribution, false);
                _battery.SetCharge(uid, 0);
                Dirty(stationEnt, bankAccountEnt);
            }
            _battery.SetMaxCharge(GetEntity(message.Entity), Convert.ToSingle(message.Now));
        }
        if (message.Max == null || message.Max <= 5000)
            return;
        else
        {
            comp.MaxLimit = (int)message.Max;
        }
    }
}
