using Content.Client.Items.Systems;
using Content.Shared.Clothing;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Inventory.Events;
using Robust.Client.GameObjects;

namespace Content.Client.DeadSpace.Arena;

/// <summary>
/// Окрашивает снаряжение с <see cref="ArenaTeamClothingComponent"/> в цвет команды TDM.
/// </summary>
public sealed class ArenaTeamClothingSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly ItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArenaTeamClothingComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<ArenaTeamClothingComponent, EquipmentVisualsUpdatedEvent>(OnVisualsUpdated);
        SubscribeLocalEvent<ArenaTeamClothingComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnAfterHandleState(Entity<ArenaTeamClothingComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ApplyTint(ent.Owner, ent.Comp);
    }

    private void OnVisualsUpdated(Entity<ArenaTeamClothingComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        var color = ArenaConstants.GetTeamColor(ent.Comp.Team);
        if (color == null)
            return;

        if (!TryComp<SpriteComponent>(args.Equipee, out var sprite))
            return;

        foreach (var key in args.RevealedLayers)
        {
            _spriteSystem.LayerSetColor((args.Equipee, sprite), key, color.Value);
        }
    }

    private void OnGotUnequipped(Entity<ArenaTeamClothingComponent> ent, ref GotUnequippedEvent args)
    {
        ResetTint(ent.Owner);
    }

    private void ApplyTint(EntityUid uid, ArenaTeamClothingComponent component)
    {
        var color = ArenaConstants.GetTeamColor(component.Team);
        if (color == null)
            return;

        if (TryComp<SpriteComponent>(uid, out var sprite))
            _spriteSystem.SetColor((uid, sprite), color.Value);

        // Принудительно пересоздать надетые слои на владельце: в EquipmentVisualsUpdatedEvent
        // будет применён цвет команды.
        _itemSystem.VisualsChanged(uid);
    }

    private void ResetTint(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _spriteSystem.SetColor((uid, sprite), Color.White);
    }
}
