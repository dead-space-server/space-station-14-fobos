using Robust.Client.GameObjects;
using Content.Shared.DeadSpace.LawConfigurator.Components;

namespace Content.Client.DeadSpace.LawConfigurator;

public sealed class LawConfiguratorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LawConfiguratorComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, LawConfiguratorComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateSprite(uid, component);
    }

    private void UpdateSprite(EntityUid uid, LawConfiguratorComponent component)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        // Используем значения из компонента
        var state = component.HasBoard ? component.FilledState : component.EmptyState;
        sprite.LayerSetState(0, state);
    }
}
