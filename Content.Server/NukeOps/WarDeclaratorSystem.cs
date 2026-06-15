using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.NukeOps;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.NukeOps;

/// <summary>
///     This handles nukeops special war mode declaration device and directly using nukeops game rule
/// </summary>
public sealed class WarDeclaratorSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!; // DS14
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WarDeclaratorComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<WarDeclaratorComponent, ActivatableUIOpenAttemptEvent>(OnAttemptOpenUI);
        SubscribeLocalEvent<WarDeclaratorComponent, WarDeclaratorActivateMessage>(OnActivated);
    }

    private void OnMapInit(Entity<WarDeclaratorComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Message = Loc.GetString(ent.Comp.Message); // DS14 дурак
        ent.Comp.DisableAt = _gameTiming.CurTime + TimeSpan.FromMinutes(ent.Comp.WarDeclarationDelay);
    }

    private void OnAttemptOpenUI(Entity<WarDeclaratorComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!_accessReaderSystem.IsAllowed(args.User, ent))
        {
            if (!args.Silent)
            {
                var msg = Loc.GetString("war-declarator-not-working");
                _popupSystem.PopupEntity(msg, ent);
            }
            args.Cancel();
            return;
        }

        UpdateUI(ent, ent.Comp.CurrentStatus);
    }

    private void OnActivated(Entity<WarDeclaratorComponent> ent, ref WarDeclaratorActivateMessage args)
    {
        var ev = new WarDeclaredEvent(ent.Comp.CurrentStatus, ent);
        RaiseLocalEvent(ref ev);

        // if (ent.Comp.DisableAt < _gameTiming.CurTime)
        //     ev.Status = WarConditionStatus.NoWarTimeout;

        ent.Comp.CurrentStatus = ev.Status;

        var maxLength = _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength);
        var message = SharedChatSystem.SanitizeAnnouncement(args.Message, maxLength);
        if (ent.Comp.AllowEditingMessage && message != string.Empty)
            ent.Comp.Message = message;

        if (ev.Status == WarConditionStatus.WarReady)
        {
            var title = Loc.GetString(ent.Comp.SenderTitle);
            // DS14-start
            _chat.DispatchGlobalAnnouncement(ent.Comp.Message, title, false, colorOverride: ent.Comp.Color);
            _sound.PlayAlertLevelGlobal(Filter.Broadcast(), ent.Comp.Sound, ent.Comp.Sound.Params);
            // DS14-end
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(args.Actor):player} has declared war with this text: {ent.Comp.Message}");
        }

        UpdateUI(ent, ev.Status);
    }

    private void UpdateUI(Entity<WarDeclaratorComponent> ent, WarConditionStatus? status = null)
    {
        _userInterfaceSystem.SetUiState(
            ent.Owner,
            WarDeclaratorUiKey.Key,
            new WarDeclaratorBoundUserInterfaceState(status, ent.Comp.DisableAt, ent.Comp.ShuttleDisabledTime));
    }
}
