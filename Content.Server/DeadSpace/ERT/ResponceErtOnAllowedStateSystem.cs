// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.ERT.Components;
using Content.Shared.Mobs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Content.Server.EUI;
using Content.Shared.Pinpointer;
using Content.Server.Roles;
using Content.Server.Mind;

namespace Content.Server.DeadSpace.ERT;

public sealed class ResponceErtOnAllowedStateSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ErtResponceSystem _ertResponceSystem = default!;
    [Dependency] private readonly SharedPinpointerSystem _sharedPinpointerSystem = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResponceErtOnAllowedStateComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnMobStateChange(Entity<ResponceErtOnAllowedStateComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.IsReady)
            return;

        if (!_playerManager.TryGetSessionByEntity(ent, out var session))
            return;

        if (!ent.Comp.AllowedStates.Contains(args.NewMobState))
            return;

        var mind = _mindSystem.GetMind(ent);
        if (mind == null)
            return;

        var title = $"Оповещение CriticalForce: {args.NewMobState}";
        string text;

        if (_roleSystem.MindIsAntagonist(mind))
        {
            text =
                "Мы обнаружили, что ваше состояние здоровья критическое.\n\n" +
                "Согласно нашим данным, вы числитесь как **противник контрагента NanoTrasen**. " +
                "К сожалению, на данный момент мы не можем напрямую оказать вам медицинскую помощь.\n\n" +
                "Однако вы можете воспользоваться сторонними медицинскими услугами " +
                "или попытаться стабилизировать состояние самостоятельно.";
        }
        else
        {
            text =
                "Мы обнаружили, что ваше состояние здоровья критическое.\n\n" +
                "Отправить отряд поддержки CriticalForce к вашему местоположению?";
        }

        _euiManager.OpenEui(
            new YesNoEui(ent.Owner, this, title, text),
            session
        );
    }

    // Обработчик ответа от EUI.
    public void HandleYesNoResponse(EntityUid target, ICommonSession player, bool accepted)
    {
        var name = player.Name ?? "(unknown)";
        var saw = _logManager.GetSawmill("yesno");
        saw.Info($"YesNo response for {target}: from {name} accepted={accepted}");

        if (!accepted)
            return;

        if (!TryComp<ResponceErtOnAllowedStateComponent>(player.AttachedEntity, out var component))
            return;

        var mind = _mindSystem.GetMind(player.AttachedEntity.Value);
        if (mind == null)
            return;

        if (_roleSystem.MindIsAntagonist(mind))
        {
            RemComp<ResponceErtOnAllowedStateComponent>(player.AttachedEntity.Value);
            return;
        }

        var ent = _ertResponceSystem.EnsureErtTeam(component.Team, false);

        if (!TryComp<RuleGridsComponent>(ent, out var ruleGrids))
            return;

        var query = EntityQueryEnumerator<PinpointerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var pin, out var xform))
        {
            if (xform.MapID == ruleGrids.Map)
                _sharedPinpointerSystem.SetTarget(uid, player.AttachedEntity.Value, pin);
        }

        RemComp<ResponceErtOnAllowedStateComponent>(player.AttachedEntity.Value);

    }
}
