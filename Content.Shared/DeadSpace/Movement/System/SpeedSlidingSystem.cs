// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using System.Numerics;

namespace Content.Shared.DeadSpace.Abilities.Slide;

public sealed class SpeedSlidingSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpeedSlidingComponent, DownAttemptEvent>(OnDownAttempt);
    }

    private void OnDownAttempt(Entity<SpeedSlidingComponent> ent, ref DownAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<PhysicsComponent>(ent.Owner, out var physics))
            return;

        var velocity = physics.LinearVelocity;
        if (velocity.Length() < ent.Comp.MinSlideSpeed)
            return;

        _stun.TryKnockdown(ent.Owner, TimeSpan.FromSeconds(1.2f), false, false, true);

        var direction = velocity.Normalized();
        var impulseMagnitude = ent.Comp.SlideSpeed * ent.Comp.SlideDistance * physics.Mass;
        var impulse = direction * impulseMagnitude;

        _physics.SetLinearVelocity(ent.Owner, Vector2.Zero);
        _physics.ApplyLinearImpulse(ent.Owner, impulse);

        _audio.PlayPredicted(ent.Comp.SlideSound, ent.Owner, ent.Owner);
    }
}