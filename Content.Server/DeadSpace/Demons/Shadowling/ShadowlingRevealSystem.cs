using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Mind;
using Content.Server.Antag;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Content.Shared.Emoting;
using Content.Shared.Stunnable;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Server.Fluids.Components;
using Robust.Shared.Map;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingRevealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ShadowlingRecruitSystem _recruit = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingRevealComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingRevealComponent, ShadowlingRevealEvent>(OnRevealAction);
        SubscribeLocalEvent<ShadowlingRevealComponent, ShadowlingRevealDoAfterEvent>(OnDoAfter);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingRevealComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionRevealEntity, component.ActionReveal);
    }

    private void OnRevealAction(EntityUid uid, ShadowlingRevealComponent component, ShadowlingRevealEvent args)
    {
        if (args.Handled) return;

        SpawnShadowlingSmoke(uid, 20f, 30);

        var sound = new SoundPathSpecifier("/Audio/Misc/ratvar_rises.ogg");
        _antag.SendBriefing(uid, "", Color.Red, sound);
        _popup.PopupEntity("Ваше тело содрогается, истинная форма рвется наружу!", uid, uid, PopupType.LargeCaution);
        _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(14));

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.Duration, new ShadowlingRevealDoAfterEvent(), uid)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, ShadowlingRevealComponent component, ShadowlingRevealDoAfterEvent args)
    {
        if (args.Cancelled) return;

        var xform = Transform(uid);
        DropItems(uid);

        var newMob = Spawn("MobShadowling", xform.Coordinates);
        EnsureComp<EmotingComponent>(newMob);

        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();
        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master == uid)
                slave.Master = newMob;
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _mind.TransferTo(mindId, newMob, mind: mind);

        if (TryComp<ShadowlingRecruitComponent>(newMob, out var recruit))
            _recruit.UpdateSlaveCount(newMob, recruit);

        QueueDel(uid);
    }

    public void SpawnShadowlingSmoke(EntityUid uid, float duration, int spread)
    {
        var xform = Transform(uid);
        if (xform.GridUid == null)
            return;

        var smoke = Spawn("Smoke", xform.Coordinates);
        if (TryComp<SmokeComponent>(smoke, out var smokeComp))
            _smoke.StartSmoke(smoke, new Solution(), duration, spread, smokeComp);
    }

    public void DropItems(EntityUid uid)
    {
        if (!_inventory.TryGetSlots(uid, out var slots)) return;
        foreach (var slot in slots)
        {
            _inventory.TryUnequip(uid, slot.Name, true, true);
        }
    }
}
