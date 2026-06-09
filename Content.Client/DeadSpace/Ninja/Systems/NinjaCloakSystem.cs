using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.DeadSpace.Ninja.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Client.DeadSpace.Ninja.Systems;

public sealed class NinjaCloakSystem : SharedNinjaCloakSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NinjaCloakComponent, AfterAutoHandleStateEvent>(OnStateChanged);
    }

    private void OnStateChanged(Entity<NinjaCloakComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var parent = Transform(ent.Owner).ParentUid;

        if (!TryComp<SpriteComponent>(parent, out var parentSprite))
            return;

        if (ent.Comp.Enabled)
        {
            _sprite.SetVisible((parent, parentSprite), false);
        }
        else
        {
            _sprite.SetVisible((parent, parentSprite), true);
        }
    }
}