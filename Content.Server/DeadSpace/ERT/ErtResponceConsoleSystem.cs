// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Content.Server.DeadSpace.ERTCall;
using Content.Shared.DeadSpace.ERT;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Server.Chat.Systems;

namespace Content.Server.DeadSpace.ERT;

public sealed class ErtResponceConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ErtResponceSystem _ertResponceSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ErtResponceConsoleComponent, ErtResponceConsoleUiButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<ErtResponceConsoleComponent, AfterActivatableUIOpenEvent>(OnUIOpen);
        SubscribeLocalEvent<ErtResponceConsoleComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnButtonPressed(EntityUid uid, ErtResponceConsoleComponent component, ErtResponceConsoleUiButtonPressedMessage args)
    {
        if (!_powerReceiverSystem.IsPowered(uid))
            return;

        if (string.IsNullOrEmpty(args.Team))
            return;

        var station = _station.GetOwningStation(uid);
        if (station == null)
            return;

        if (!TryComp<StationBankAccountComponent>(station, out var stationAccount))
            return;

        switch (args.Button)
        {
            case ErtResponceConsoleUiButton.ResponceErt:
                {
                    var price = _ertResponceSystem.GetErtPrice(args.Team);

                    var balance = _cargoSystem.GetBalanceFromAccount(
                        (station.Value, stationAccount),
                        component.Account
                    );

                    if (balance < price)
                        return;

                    if (_ertResponceSystem.TryCallErt(args.Team))
                        _cargoSystem.UpdateBankAccount(
                            (station.Value, stationAccount),
                            -price,
                            component.Account
                        );
                    else
                        _chatSystem.TrySendInGameICMessage(
                            uid,
                            Loc.GetString("ert-responce-call-cancel"),
                            InGameICChatType.Speak,
                            ChatTransmitRange.Normal,
                            true
                        );


                    break;
                }

            default:
                break;
        }

        UpdateUserInterface((uid, component));
    }

    private void OnPowerChanged(EntityUid uid, ErtResponceConsoleComponent component, ref PowerChangedEvent args)
    {
        UpdateUserInterface((uid, component));
    }


    private void OnUIOpen(EntityUid uid, ErtResponceConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateUserInterface((uid, component));
    }

    public void UpdateUserInterface(Entity<ErtResponceConsoleComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!TryComp<UserInterfaceComponent>(entity, out var userInterface))
            return;

        if (!_uiSystem.HasUi(entity, ErtResponceConsoleUiKey.Key, userInterface))
            return;

        if (!_powerReceiverSystem.IsPowered(entity))
        {
            _uiSystem.CloseUis((entity, userInterface));
            return;
        }

        var newState = GetUserInterfaceState((entity, entity.Comp));
        _uiSystem.SetUiState((entity, userInterface), ErtResponceConsoleUiKey.Key, newState);
    }

    private ErtResponceConsoleBoundUserInterfaceState GetUserInterfaceState(Entity<ErtResponceConsoleComponent?> console)
    {
        if (!Resolve(console, ref console.Comp, false))
            return default!;

        var station = _station.GetOwningStation(console);

        var balance = 0;

        if (TryComp<StationBankAccountComponent>(station, out var stationAccount))
            balance = _cargoSystem.GetBalanceFromAccount((station.Value, stationAccount), console.Comp.Account);

        return new ErtResponceConsoleBoundUserInterfaceState(
            console.Comp.Teams,
            balance
        );
    }


}

