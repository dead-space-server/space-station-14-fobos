using Content.Server.Humanoid;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Popups;
using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Server.GameObjects;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class NinjaScannerSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly NameModifierSystem _nameMod = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedChameleonClothingSystem _chameleon = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NinjaScannerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NinjaScannerComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<NinjaScannerComponent, NinjaScanActionEvent>(OnScan);
        SubscribeLocalEvent<NinjaScannerComponent, NinjaOpenScannerActionEvent>(OnOpenUi);
        SubscribeLocalEvent<NinjaScannerComponent, NinjaApplyDisguiseMessage>(OnApplyDisguise);
    }

    private void OnMapInit(Entity<NinjaScannerComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ScanActionEntity, comp.ScanAction);
        _actions.AddAction(uid, ref comp.OpenUiActionEntity, comp.OpenUiAction);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<NinjaScannerComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.ScanActionEntity);
        args.AddAction(ent.Comp.OpenUiActionEntity);
    }

    private void OnOpenUi(Entity<NinjaScannerComponent> ent, ref NinjaOpenScannerActionEvent args)
    {
        args.Handled = true;
        _ui.OpenUi(ent.Owner, NinjaScannerUiKey.Key, args.Performer);
        UpdateUi(ent);
    }

    private void OnScan(Entity<NinjaScannerComponent> ent, ref NinjaScanActionEvent args)
    {
        var target = args.Target;
        if (target == EntityUid.Invalid || !Exists(target))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("ninja-scanner-invalid-target"), ent, args.Performer);
            return;
        }

        args.Handled = true;

        var name = MetaData(target).EntityName;
        var data = new NinjaScanData(name, GetNetEntity(target));

        ent.Comp.ScannedTargets.RemoveAll(d => d.Target == data.Target);

        ent.Comp.ScannedTargets.Insert(0, data);
        while (ent.Comp.ScannedTargets.Count > ent.Comp.MaxScans)
            ent.Comp.ScannedTargets.RemoveAt(ent.Comp.ScannedTargets.Count - 1);

        Dirty(ent);

        _popup.PopupEntity(Loc.GetString("ninja-scanner-scan-success", ("target", name)), ent, args.Performer);
    }

    private void OnApplyDisguise(Entity<NinjaScannerComponent> ent, ref NinjaApplyDisguiseMessage args)
    {
        var performer = args.Actor;
        var target = GetEntity(args.Target);

        if (!Exists(target))
            return;

        ApplyDisguise(target, performer);
    }
    private void ApplyDisguise(EntityUid target, EntityUid performer)
    {
        if (HasComp<HumanoidAppearanceComponent>(target) && HasComp<HumanoidAppearanceComponent>(performer))
        {
            _humanoid.CloneAppearance(target, performer);

            if (TryComp<HumanoidAppearanceComponent>(target, out var targetHumanoid) && HasComp<InventoryComponent>(performer))
            {
                _inventory.SetInventorySpecies(performer, targetHumanoid.Species);
            }
        }
        var targetName = _nameMod.GetBaseName(target);
        _metaData.SetEntityName(performer, targetName);

        CopyChameleonClothing(target, performer);

        _popup.PopupEntity(Loc.GetString("ninja-scanner-disguise-success", ("target", MetaData(target).EntityName)), performer, performer);
    }

    private void UpdateUi(Entity<NinjaScannerComponent> ent)
    {
        if (!_ui.HasUi(ent.Owner, NinjaScannerUiKey.Key))
            return;

        _ui.SetUiState(ent.Owner, NinjaScannerUiKey.Key, new NinjaScannerBoundUserInterfaceState(ent.Comp.ScannedTargets));
    }

    private void CopyChameleonClothing(EntityUid target, EntityUid performer)
    {
        if (!_inventory.TryGetSlots(target, out var targetSlots))
            return;

        foreach (var slot in targetSlots)
        {
            if (!_inventory.TryGetSlotEntity(target, slot.Name, out var targetItem))
                continue;

            if (!_inventory.TryGetSlotEntity(performer, slot.Name, out var ninjaItem))
                continue;

            if (TryComp<ChameleonClothingComponent>(ninjaItem, out var chameleon))
                _chameleon.SetSelectedPrototype(ninjaItem.Value, MetaData(targetItem.Value).EntityPrototype!.ID, component: chameleon);

            if (TryComp<ClothingComponent>(targetItem.Value, out var targetClothing) &&
                TryComp<ClothingComponent>(ninjaItem.Value, out var ninjaClothing))
            {
                _clothing.CopyVisuals(ninjaItem.Value, targetClothing, ninjaClothing);
            }
        }
    }
}