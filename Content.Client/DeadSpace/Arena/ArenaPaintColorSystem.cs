using Content.Client.Items.Systems;
using Content.Shared.Clothing;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Inventory.Events;
using Robust.Client.GameObjects;

namespace Content.Client.DeadSpace.Arena;

/// <summary>
/// Окрашивает тело и снаряжение с <see cref="ArenaPaintColorComponent"/> в цвет команды пеинтболла.
/// </summary>
public sealed class ArenaPaintColorSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly ItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArenaPaintColorComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<ArenaPaintColorComponent, EquipmentVisualsUpdatedEvent>(OnVisualsUpdated);
        SubscribeLocalEvent<ArenaPaintColorComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnAfterHandleState(Entity<ArenaPaintColorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ApplyTint(ent.Owner, ent.Comp);
    }

    private void OnVisualsUpdated(Entity<ArenaPaintColorComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        if (ent.Comp.Team == ArenaTeam.None)
            return;

        if (!TryComp<SpriteComponent>(args.Equipee, out var sprite))
            return;

        foreach (var key in args.RevealedLayers)
        {
            _spriteSystem.LayerSetColor((args.Equipee, sprite), key, ent.Comp.Color);
        }
    }

    private void OnGotUnequipped(Entity<ArenaPaintColorComponent> ent, ref GotUnequippedEvent args)
    {
        ResetTint(ent.Owner);
    }

    private void ApplyTint(EntityUid uid, ArenaPaintColorComponent component)
    {
        if (component.Team == ArenaTeam.None)
            return;

        if (TryComp<SpriteComponent>(uid, out var sprite))
            _spriteSystem.SetColor((uid, sprite), component.Color);

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