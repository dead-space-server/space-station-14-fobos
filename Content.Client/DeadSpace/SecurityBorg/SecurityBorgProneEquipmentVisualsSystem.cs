using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.DeadSpace.SecurityBorg;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client.DeadSpace.SecurityBorg;

public sealed class SecurityBorgProneEquipmentVisualsSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
    }

    private void OnEquipmentVisualsUpdated(Entity<ItemComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        if (!TryComp<SecurityBorgProneComponent>(args.Equipee, out var securityBorgProne)
            || !_appearance.TryGetData<bool>(args.Equipee, SecurityBorgProneVisuals.Prone, out var prone)
            || !prone
            || !TryComp<SpriteComponent>(args.Equipee, out var sprite))
        {
            return;
        }

        foreach (var key in args.RevealedLayers)
        {
            if (!_sprite.LayerMapTryGet((args.Equipee, sprite), key, out var index, false)
                || sprite[index] is not SpriteComponent.Layer layer)
            {
                continue;
            }

            _sprite.LayerSetOffset(layer, layer.Offset + securityBorgProne.HeadSlotProneOffsetDelta);
        }
    }
}
