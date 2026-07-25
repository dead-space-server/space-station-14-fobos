// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.DeadSpace.Clothing;
using Content.Client.Inventory;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.DeadSpace.Sandevistan;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Sandevistan;

public sealed class SandevistanBodyVisualSystem : EntitySystem
{
    private static readonly ResPath SpritePath =
        new("/Textures/_DeadSpace/Mobs/Effects/sandevistan.rsi");

    private const string SpriteState = "body";
    private const string BackSlot = "back";
    private const string OuterClothingSlot = "outerClothing";

    [Dependency] private readonly ClientHideLayerClothingSystem _hideLayerClothing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanBodyVisualComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SandevistanBodyVisualComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ClothingComponent, EquipmentVisualsUpdatedEvent>(
            OnEquipmentVisualsUpdated,
            after: [typeof(ClientHideLayerClothingSystem)]);
    }

    private void OnStartup(Entity<SandevistanBodyVisualComponent> ent, ref ComponentStartup args)
    {
        SetBackVisualsVisible(ent.Owner, false);

        if (!TryComp<SpriteComponent>(ent, out var sprite) ||
            _sprite.LayerMapTryGet((ent.Owner, sprite), SandevistanBodyVisualLayers.Body, out _, false) ||
            !_sprite.LayerMapTryGet((ent.Owner, sprite), OuterClothingSlot, out var outerClothingLayer, false))
        {
            return;
        }

        var layer = _sprite.AddLayer(
            (ent.Owner, sprite),
            new SpriteSpecifier.Rsi(SpritePath, SpriteState),
            outerClothingLayer);

        _sprite.LayerMapSet((ent.Owner, sprite), SandevistanBodyVisualLayers.Body, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }

    private void OnShutdown(Entity<SandevistanBodyVisualComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite) &&
            _sprite.LayerMapTryGet((ent.Owner, sprite), SandevistanBodyVisualLayers.Body, out _, false))
        {
            _sprite.RemoveLayer((ent.Owner, sprite), SandevistanBodyVisualLayers.Body);
        }

        _hideLayerClothing.RefreshSlots(ent.Owner, BackSlot);
    }

    private void OnEquipmentVisualsUpdated(Entity<ClothingComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        if (!args.Slot.Equals(BackSlot, StringComparison.OrdinalIgnoreCase) ||
            !HasComp<SandevistanBodyVisualComponent>(args.Equipee) ||
            !TryComp<SpriteComponent>(args.Equipee, out var sprite))
        {
            return;
        }

        SetLayersVisible(args.Equipee, sprite, args.RevealedLayers, false);
    }

    private void SetBackVisualsVisible(EntityUid wearer, bool visible)
    {
        if (!TryComp<InventorySlotsComponent>(wearer, out var inventorySlots) ||
            !TryComp<SpriteComponent>(wearer, out var sprite) ||
            !inventorySlots.VisualLayerKeys.TryGetValue(BackSlot, out var layers))
        {
            return;
        }

        SetLayersVisible(wearer, sprite, layers, visible);
    }

    private void SetLayersVisible(
        EntityUid wearer,
        SpriteComponent sprite,
        IEnumerable<string> layers,
        bool visible)
    {
        foreach (var layerKey in layers)
        {
            if (!_sprite.LayerMapTryGet((wearer, sprite), layerKey, out var layer, false))
                continue;

            _sprite.LayerSetVisible((wearer, sprite), layer, visible);
        }
    }

    private enum SandevistanBodyVisualLayers : byte
    {
        Body,
    }
}
