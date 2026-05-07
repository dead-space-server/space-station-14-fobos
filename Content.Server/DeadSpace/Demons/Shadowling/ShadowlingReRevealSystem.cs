// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Stunnable;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingReRevealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingReRevealComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingReRevealComponent, ShadowlingReRevealEvent>(OnReReveal);
        SubscribeLocalEvent<ShadowlingReRevealComponent, ShadowlingReRevealDoAfterEvent>(OnDoAfter);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingReRevealComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionReRevealEntity, component.ActionReReveal);
    }

    private void OnReReveal(EntityUid uid, ShadowlingReRevealComponent component, ShadowlingReRevealEvent args)
    {
        if (args.Handled) return;

        _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(5));

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(component.Duration), new ShadowlingReRevealDoAfterEvent(), uid)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, ShadowlingReRevealComponent component, ShadowlingReRevealDoAfterEvent args)
    {
        if (args.Cancelled) return;

        if (TryComp<DamageableComponent>(uid, out _))
            _damageable.SetAllDamage(uid, FixedPoint2.Zero);
    }
}