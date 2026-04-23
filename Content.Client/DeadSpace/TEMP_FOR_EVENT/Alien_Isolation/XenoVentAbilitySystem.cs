using Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Client.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation;

public sealed class XenoVentAbilitySystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenoVentAbilityComponent, AfterAutoHandleStateEvent>(OnComponentStateUpdated);
    }

    private void OnComponentStateUpdated(EntityUid uid, XenoVentAbilityComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.SetVisible((uid, sprite), !comp.IsActive);
    }
}
