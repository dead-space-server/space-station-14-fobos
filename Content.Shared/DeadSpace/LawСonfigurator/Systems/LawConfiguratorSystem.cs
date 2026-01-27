using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DeadSpace.LawConfigurator.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;

namespace Content.Shared.DeadSpace.LawConfigurator.Systems;

public sealed class LawConfiguratorSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LawConfiguratorComponent, AfterInteractEvent>(OnAfterInteract); // Тыкнули
        SubscribeLocalEvent<LawConfiguratorComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged); // Вставили
        SubscribeLocalEvent<LawConfiguratorComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged); // Высунули
        SubscribeLocalEvent<LawConfiguratorComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, LawConfiguratorComponent component, ComponentInit args)
    {
        UpdateBoardState(uid);
    }

    private void OnItemSlotChanged(EntityUid uid, LawConfiguratorComponent component, ContainerModifiedMessage args)
    {
        if (args.Container.ID != "circuit_holder")
            return;

        UpdateBoardState(uid);
    }

    private void UpdateBoardState(EntityUid uid)
    {
        if (!TryComp<LawConfiguratorComponent>(uid, out var component))
            return;

        var hasBoard = _itemSlots.TryGetSlot(uid, "circuit_holder", out var slot) && slot.Item != null;
        
        if (component.HasBoard != hasBoard)
        {
            component.HasBoard = hasBoard;
            Dirty(uid, component);
        }
    }

    private void OnAfterInteract(EntityUid uid, LawConfiguratorComponent comp, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        if (!TryComp<SiliconLawBoundComponent>(target, out var siliconLaw))
            return;
        
        if (!_itemSlots.TryGetSlot(uid, "circuit_holder", out var slot) || slot.Item == null)
        {
            _popup.PopupClient(
                Loc.GetString("law-configurator-requires-board"),
                args.User,
                args.User);
            return;
        }

        var targetName = Identity.Name(target, EntityManager);
        _popup.PopupClient(Loc.GetString("law-configurator-start-configuring", ("target", targetName)), 
            args.User,
            args.User);
    }
}