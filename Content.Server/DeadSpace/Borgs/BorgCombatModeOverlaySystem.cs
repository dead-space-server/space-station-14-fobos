// DS14
// DS14-Start
// Система управления оверлеем боевого режима для боргов
//    Отслеживает изменения CombatModeComponent через CombatModeChangedEvent
//    и устанавливает данные Appearance для переключения слоя combat_overlay.
//    На клиенте GenericVisualizer обрабатывает эти данные и меняет sprite.
// DS14-End
using Content.Shared.CombatMode;
using Content.Shared.DeadSpace.Borgs;

namespace Content.Server.DeadSpace.Borgs;

public sealed class BorgCombatModeOverlaySystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgCombatModeOverlayComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BorgCombatModeOverlayComponent, CombatModeChangedEvent>(OnCombatModeChanged);
    }

    private void OnInit(Entity<BorgCombatModeOverlayComponent> ent, ref ComponentInit args)
    {
        if (TryComp<CombatModeComponent>(ent, out var combat))
            SetCombatVisuals(ent, combat.IsInCombatMode);
    }

    private void OnCombatModeChanged(Entity<BorgCombatModeOverlayComponent> ent, ref CombatModeChangedEvent args)
    {
        SetCombatVisuals(ent, args.IsInCombatMode);
    }

    private void SetCombatVisuals(EntityUid uid, bool isInCombat)
    {
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, BorgCombatModeVisuals.Combat, isInCombat, appearance);
    }
}
