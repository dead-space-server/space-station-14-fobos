using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Mind;
using Content.Server.Chat.Systems;
using Content.Shared.Emoting;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Stunnable;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Server.Fluids.Components;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingAscendanceComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, ShadowlingAscendanceEvent>(OnAscendanceAction);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, ShadowlingAscendanceDoAfterEvent>(OnDoAfter);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingAscendanceComponent component, ComponentInit args)
    {
    }

    private void OnAscendanceAction(EntityUid uid, ShadowlingAscendanceComponent component, ShadowlingAscendanceEvent args)
    {
        if (args.Handled) return;

        var xform = Transform(uid);
        if (xform.GridUid != null)
        {
            var smoke = Spawn("Smoke", xform.Coordinates);
            if (TryComp<SmokeComponent>(smoke, out var smokeComp))
                _smoke.StartSmoke(smoke, new Solution(), 20f, 30, smokeComp);
        }

        _audio.PlayPvs("/Audio/Misc/ratvar_rises.ogg", uid);
        _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(component.Duration + 0.5f));

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.Duration, new ShadowlingAscendanceDoAfterEvent(), uid)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, ShadowlingAscendanceComponent component, ShadowlingAscendanceDoAfterEvent args)
    {
        if (args.Cancelled) return;

        var xform = Transform(uid);
        var newMob = Spawn("MobShadowlingAscended", xform.Coordinates);
        EnsureComp<EmotingComponent>(newMob);

        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();
        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master == uid)
                slave.Master = newMob;
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, newMob, mind: mind);

        var message = "Внимание! Мы фиксируем вознесение тенеморфа на вашей станции! Сектор полностью изолирован, любые попытки покинуть его будут пресекаться огнём Блюспейс Артиллерии. Для локализации и нейтрализации угрозы будет задействован флот NanoTrasen. Благодарим за сотрудничество и преданность корпорации!";
        var sender = "Департамент Вооружённых Сил NanoTrasen";
        _chat.DispatchGlobalAnnouncement(message, sender, colorOverride: Color.FromHex("#ff0000"), announcementSound: new SoundPathSpecifier("/Audio/_DeadSpace/Demons/Shadowling.ogg"));

        QueueDel(uid);
    }
}
