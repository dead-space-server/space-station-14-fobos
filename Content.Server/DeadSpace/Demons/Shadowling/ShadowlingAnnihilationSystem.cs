using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Humanoid;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingAnnihilationSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingAnnihilationComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingAnnihilationComponent, ShadowlingAnnihilationEvent>(OnAnnihilationAction);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingAnnihilationComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionAnnihilationEntity, component.ActionAnnihilation);
    }

    private void OnAnnihilationAction(EntityUid uid, ShadowlingAnnihilationComponent component, ShadowlingAnnihilationEvent args)
    {
        if (args.Handled) return;

        var target = args.Target;
        var performer = args.Performer;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        if (target == performer || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingRevealComponent>(target))
            return;
        if (TryComp<MetaDataComponent>(target, out var meta) && meta.EntityPrototype?.ID == "MobHumanDeathSquadUnit")
            return;

        var targetPos = _transform.GetMapCoordinates(target).Position;
        var performerPos = _transform.GetMapCoordinates(performer).Position;
        var direction = targetPos - performerPos;
        if (direction.LengthSquared() > 0)
        {
            var impulseVector = direction.Normalized() * 10000f;
            _physics.ApplyLinearImpulse(target, impulseVector);
        }

        if (TryComp<BodyComponent>(target, out var body))
        {
            _body.GibBody(target, true, body);
        }

        if (Exists(target))
            QueueDel(target);

        args.Handled = true;
    }
}
