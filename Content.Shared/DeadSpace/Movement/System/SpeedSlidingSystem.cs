// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.DeadSpace.Abilities.Slide;

public sealed class SpeedSlidingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public bool TrySlide(EntityUid uid)
    {
        if (!TryComp<SpeedSlidingComponent>(uid, out var slide))
            return false;

        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return false;

        var velocity = physics.LinearVelocity;
        var speed = velocity.Length();
        if (speed < slide.MinSlideSpeed)
            return false;

        var direction = velocity / speed;
        var slideVelocity = direction * (speed + slide.SlideSpeed);

        _physics.SetLinearVelocity(uid, slideVelocity);

        _audio.PlayPredicted(slide.SlideSound, uid, uid);
        return true;
    }
}
