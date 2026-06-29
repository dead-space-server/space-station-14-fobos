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
using Robust.Server.GameObjects;

namespace Content.Server.DeadSpace.EnergySeller;

public sealed partial class EnergySellerSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, BatteryStateChangedEvent>(CheckBatteryCharges);
        SubscribeLocalEvent<EnergySellerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<EnergySellerComponent, ChangesForSellingEnergy>(ChoseVoid);
        SubscribeLocalEvent<EnergySellerComponent, ComponentStartup>(WorkWithDictionaryDistibution);
    }
    private void WorkWithDictionaryDistibution(EntityUid uid, EnergySellerComponent comp, ComponentStartup args)
    {
        comp.Distribution = new Dictionary<ProtoId<CargoAccountPrototype>, double>
                                {
                                    { comp.Account, 1.0 },
                                };
    }
    private void OnInit(EntityUid uid, EnergySellerComponent component, ComponentInit args)
    {
        UpdateUI(uid, component);
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
    private void ChoseVoid(EntityUid uid, EnergySellerComponent comp, ChangesForSellingEnergy message)
    {
        if (message.SpeedOrLimit)
        {
            SetSpeed(uid, comp, message);
        }
        else
        {
            SetMaxLimit(uid, comp, message);
        }
    }
    private void SetSpeed(EntityUid uid, EnergySellerComponent comp, ChangesForSellingEnergy message)
    {
        if (!(message.Now is null) && message.Now >= 5000 && TryComp<PowerNetworkBatteryComponent>(GetEntity(message.Entity), out var compSell))
        {
            compSell.MaxSupply = Convert.ToSingle(message.Now);
        }
        if (message.Max != null && message.Max >= 5000)
        {
            comp.MaxChargeRate = (int)message.Max;
        }
        UpdateUI(uid, comp);
    }
    private void SetMaxLimit(EntityUid uid, EnergySellerComponent comp, ChangesForSellingEnergy message)
    {
        if (message.Now is not null && message.Now >= 5000 && TryComp<BatteryComponent>(GetEntity(message.Entity), out var compSell))
        {
            if (message.Now > compSell.LastCharge)
            {
                var station = _station.GetOwningStation(uid);

                StationBankAccountComponent? bankAccount = null;
                if (station != null)
                    TryComp(station.Value, out bankAccount);

                var bankAccountEnt = bankAccount!;
                var stationEnt = station!.Value;

                _cargo.UpdateBankAccount((stationEnt, bankAccountEnt), (int)Math.Round(compSell.PricePerJoule * compSell.MaxCharge + (compSell.MaxCharge / comp.AdditionalCoefficient + 1)), comp.Distribution, false);
                _battery.SetCharge(uid, 0);
                Dirty(stationEnt, bankAccountEnt);
            }
            _battery.SetMaxCharge(GetEntity(message.Entity), Convert.ToSingle(message.Now));
        }
        if (message.Max != null && message.Max >= 5000)
        {
            comp.MaxLimit = (int)message.Max;
        }
        UpdateUI(uid, comp);
    }
    private void UpdateUI(EntityUid uid, EnergySellerComponent comp)
    {
        if (!_userInterfaceSystem.HasUi(uid, ESBControllerUiKey.Key))
            return;
        if (!TryComp<BatteryComponent>(uid, out var compBat))
            return;
        if (!TryComp<PowerNetworkBatteryComponent>(uid, out var compSell))
            return;
        _userInterfaceSystem.SetUiState(uid, ESBControllerUiKey.Key, new EnergySellerBoundUserInterfaceState(comp.MaxChargeRate, comp.MaxLimit, (int)compSell.MaxChargeRate, (int)compBat.MaxCharge));
    }
}
