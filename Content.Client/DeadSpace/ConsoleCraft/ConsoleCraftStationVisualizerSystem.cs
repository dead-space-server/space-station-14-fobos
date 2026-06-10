using Content.Shared.DeadSpace.ConsoleCraft;
using Robust.Client.GameObjects;

namespace Content.Client.DeadSpace.ConsoleCraft;

public sealed class ConsoleCraftStationVisualizerSystem : VisualizerSystem<ConsoleCraftStationComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, ConsoleCraftStationComponent comp,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData(uid, ConsoleCraftStationVisuals.Working,
                out ConsoleCraftStationVisualState state, args.Component))
            return;

        args.Sprite.LayerSetVisible("working", state == ConsoleCraftStationVisualState.Crafting);
        args.Sprite.LayerSetAnimationTime("working", 0f); // сброс анимации
    }
}