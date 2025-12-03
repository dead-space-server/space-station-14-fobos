// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Client.GameObjects;
using Content.Shared.Revenant.Components;
using Content.Shared.Humanoid;
namespace Content.Client.Revenant;

public sealed class RevenantMindCapturedSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RevenantMindCapturedComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<RevenantMindCapturedComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(EntityUid uid, RevenantMindCapturedComponent comp, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetVisible((uid, sprite), HumanoidVisualLayers.RevenantEyes, true);
    }

    private void OnComponentShutdown(EntityUid uid, RevenantMindCapturedComponent comp, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetVisible((uid, sprite), HumanoidVisualLayers.RevenantEyes, false);

    }

}
