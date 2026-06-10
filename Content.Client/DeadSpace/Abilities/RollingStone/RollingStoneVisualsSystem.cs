// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Shared.DeadSpace.Abilities;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Client.DamageState;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Abilities.Systems;

public sealed class RollingStoneVisualsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ActiveRollingStoneComponent, AfterAutoHandleStateEvent>(OnStarted);
        SubscribeLocalEvent<ActiveRollingStoneComponent, ComponentShutdown>(OnStopped);
    }

    private void OnStarted(Entity<ActiveRollingStoneComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.LayerSetVisible(DamageStateVisualLayers.Base, false);
        sprite.LayerSetVisible(RollingStoneVisualLayers.Rolling, true);
    }

    private void OnStopped(Entity<ActiveRollingStoneComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.LayerSetVisible(RollingStoneVisualLayers.Rolling, false);
        sprite.LayerSetVisible(DamageStateVisualLayers.Base, true);
    }
}