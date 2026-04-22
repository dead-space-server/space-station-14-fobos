// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using System.Reflection;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.UserInterface;
using Content.Shared.DeadSpace.ConsoleCraft;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.UserInterface;
using Content.Shared.Tools.Components;
using Content.Shared.Power;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Tag;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Tools.Systems;
using Content.Shared.Tools;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Map;
using Robust.Shared.GameObjects;

namespace Content.Server.DeadSpace.ConsoleCraft;

public sealed partial class ConsoleCraftSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private ConsoleCraftBlueprintSystem? _blueprints;
    private ConsoleCraftBlueprintSystem Blueprints =>
        _blueprints ??= EntityManager.System<ConsoleCraftBlueprintSystem>();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConsoleCraftStationComponent, ComponentInit>(OnStationInit);
        SubscribeLocalEvent<ConsoleCraftStationComponent, InteractUsingEvent>(OnStationInteractUsing);
        SubscribeLocalEvent<ConsoleCraftStationComponent, ComponentShutdown>(OnStationShutdown);
        SubscribeLocalEvent<ConsoleCraftStationComponent, PowerChangedEvent>(OnStationPowerChanged);

        SubscribeLocalEvent<ConsoleCraftConsoleComponent, AfterActivatableUIOpenEvent>(OnConsoleOpen);
        SubscribeLocalEvent<ConsoleCraftConsoleComponent, ConsoleCraftSelectBlueprintMessage>(OnSelectBlueprint);
        SubscribeLocalEvent<ConsoleCraftConsoleComponent, ConsoleCraftBackMessage>(OnBack);
        SubscribeLocalEvent<ConsoleCraftConsoleComponent, ConsoleCraftStartMessage>(OnStartCraft);
        SubscribeLocalEvent<ConsoleCraftConsoleComponent, InteractUsingEvent>(OnConsoleInteractUsing);

        SubscribeLocalEvent<CraftedItemModulesComponent, ExaminedEvent>(OnExamineCraftedModules);
        SubscribeLocalEvent<ConsoleCraftConsoleComponent, ConsoleCraftEjectMessage>(OnEjectItems);
    }

    private void OnExamineCraftedModules(EntityUid uid, CraftedItemModulesComponent comp, ExaminedEvent args)
    {
        if (comp.AppliedModules.Count == 0)
            return;

        using (args.PushGroup(nameof(CraftedItemModulesComponent)))
        {
            args.PushMarkup(Loc.GetString("consolecraft-examine-modules-header"));

            foreach (var moduleId in comp.AppliedModules)
            {
                if (!_proto.TryIndex<MinorItemModulePrototype>(moduleId, out var moduleDef))
                    continue;

                args.PushMarkup(Loc.GetString(
                    "consolecraft-examine-module-entry",
                    ("description", moduleDef.Description)));
            }
        }
    }

    private void OnStationShutdown(EntityUid uid, ConsoleCraftStationComponent comp, ComponentShutdown args)
    {
        StopCraftingSound(uid, comp);
        EjectAllItems(uid, comp);

        if (comp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(comp.LinkedConsole.Value, out var consoleComp))
        {
            consoleComp.LinkedStation = null;
            RefreshConsoleState(comp.LinkedConsole.Value, consoleComp);
        }
    }

    private void OnStationPowerChanged(EntityUid uid, ConsoleCraftStationComponent comp, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            AbortCraft(uid, comp);
        }
        else
        {
            TryStartCraft(uid, comp);
        }
    }

    private void AbortCraft(EntityUid stationUid, ConsoleCraftStationComponent comp)
    {
        if (!comp.CraftInProgress)
            return;

        comp.CraftInProgress = false;
        StopCraftingSound(stationUid, comp);
        _appearance.SetData(stationUid, ConsoleCraftStationVisuals.Working, ConsoleCraftStationVisualState.Idle);
        EjectAllItems(stationUid, comp);
        _popup.PopupEntity(Loc.GetString("consolecraft-craft-cancelled-no-power"), stationUid);

        if (comp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(comp.LinkedConsole.Value, out var consoleComp))
        {
            RefreshConsoleState(comp.LinkedConsole.Value, consoleComp);
        }
    }

    private bool TryStartCraft(EntityUid stationUid, ConsoleCraftStationComponent comp)
    {
        if (comp.CraftInProgress || !this.IsPowered(stationUid, EntityManager))
            return false;

        if (comp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(comp.LinkedConsole.Value, out var consoleComp))
        {
            RefreshConsoleState(comp.LinkedConsole.Value, consoleComp);
        }

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ConsoleCraftStationComponent>();
        while (query.MoveNext(out var stationUid, out var stationComp))
        {
            if (!stationComp.CraftInProgress)
                continue;

            if (!Transform(stationUid).Anchored)
            {
                stationComp.CraftInProgress = false;
                StopCraftingSound(stationUid, stationComp);
                _appearance.SetData(stationUid, ConsoleCraftStationVisuals.Working, ConsoleCraftStationVisualState.Idle);

                if (stationComp.LinkedConsole.HasValue &&
                    TryComp<ConsoleCraftConsoleComponent>(stationComp.LinkedConsole.Value, out var consoleComp))
                {
                    _popup.PopupEntity(Loc.GetString("consolecraft-craft-cancelled-unanchored"), stationUid);
                    RefreshConsoleState(stationComp.LinkedConsole.Value, consoleComp);
                }
                continue;
            }

            if (_timing.CurTime < stationComp.PackEndTime)
                continue;

            FinishCraft(stationUid, stationComp);
        }
    }

    private void OnStationInit(EntityUid uid, ConsoleCraftStationComponent comp, ComponentInit args)
    {
        comp.ItemContainer = _container.EnsureContainer<Container>(uid, ConsoleCraftStationComponent.ContainerName);
    }

    private void OnConsoleInteractUsing(EntityUid uid, ConsoleCraftConsoleComponent comp, InteractUsingEvent args)
    {
        if (args.Handled || !IsMultitool(args.Used))
            return;

        var linking = EnsureComp<ConsoleCraftLinkingComponent>(args.User);
        linking.ConsoleUid = uid;

        _popup.PopupEntity(Loc.GetString("consolecraft-linking-mode-started"), uid, args.User);
        args.Handled = true;
    }

    private void OnStationInteractUsing(EntityUid uid, ConsoleCraftStationComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (_tool.HasQuality(args.Used, new ProtoId<ToolQualityPrototype>("Prying"))   ||
            _tool.HasQuality(args.Used, new ProtoId<ToolQualityPrototype>("Screwing"))  ||
            _tool.HasQuality(args.Used, new ProtoId<ToolQualityPrototype>("Welding"))   ||
            _tool.HasQuality(args.Used, new ProtoId<ToolQualityPrototype>("Anchoring")))
        {
            return;
        }

        if (IsMultitool(args.Used) && TryComp<ConsoleCraftLinkingComponent>(args.User, out var linking))
        {
            CompleteLink(uid, comp, linking.ConsoleUid, args.User);
            RemComp<ConsoleCraftLinkingComponent>(args.User);
            args.Handled = true;
            return;
        }

        if (IsMultitool(args.Used))
            return;

        if (comp.CraftInProgress)
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-craft-in-progress"), uid, args.User);
            args.Handled = true;
            return;
        }

        if (comp.ActiveRecipeId == null ||
            !_proto.TryIndex<ConsoleCraftPrototype>(comp.ActiveRecipeId, out var recipe))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-no-recipe"), uid, args.User);
            args.Handled = true;
            return;
        }

        var result = TryInsertItem(uid, comp, recipe, args.Used);
        switch (result)
        {
            case InsertResult.Success:
                _popup.PopupEntity(Loc.GetString("consolecraft-item-inserted"), uid, args.User);
                args.Handled = true;
                NotifyLinkedConsole(uid, comp);
                break;

            case InsertResult.SuccessModule:
                _popup.PopupEntity(Loc.GetString("consolecraft-module-inserted"), uid, args.User);
                args.Handled = true;
                NotifyLinkedConsole(uid, comp);
                break;

            case InsertResult.ModuleFull:
                _popup.PopupEntity(Loc.GetString("consolecraft-module-slots-full"), uid, args.User);
                args.Handled = true;
                break;

            case InsertResult.ModuleDuplicate:
                _popup.PopupEntity(Loc.GetString("consolecraft-module-already-inserted"), uid, args.User);
                args.Handled = true;
                break;

            case InsertResult.WrongItem:
                _popup.PopupEntity(Loc.GetString("consolecraft-wrong-item"), uid, args.User);
                break;
        }
    }

    private void CompleteLink(EntityUid stationUid, ConsoleCraftStationComponent stationComp,
        EntityUid consoleUid, EntityUid user)
    {
        if (!TryComp<ConsoleCraftConsoleComponent>(consoleUid, out var consoleComp))
            return;

        if (consoleComp.LinkedStation.HasValue &&
            TryComp<ConsoleCraftStationComponent>(consoleComp.LinkedStation.Value, out var oldStation))
            oldStation.LinkedConsole = null;

        if (stationComp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(stationComp.LinkedConsole.Value, out var oldConsole))
            oldConsole.LinkedStation = null;

        consoleComp.LinkedStation = stationUid;
        stationComp.LinkedConsole = consoleUid;

        _popup.PopupEntity(Loc.GetString("consolecraft-linked-success"), stationUid, user);
        RefreshConsoleState(consoleUid, consoleComp);
    }

    private void OnConsoleOpen(EntityUid uid, ConsoleCraftConsoleComponent comp, AfterActivatableUIOpenEvent args)
    {
        RefreshConsoleState(uid, comp, comp.ShowingList);
    }

    private void OnSelectBlueprint(EntityUid uid, ConsoleCraftConsoleComponent comp,
        ConsoleCraftSelectBlueprintMessage msg)
    {
        if (!TryComp<ConsoleCraftBlueprintReceiverComponent>(uid, out var receiver))
            return;

        if (!Blueprints.GetAvailableRecipeIds((uid, receiver)).Contains(msg.RecipeId))
            return;

        if (TryGetLinkedStation(uid, comp, out _, out var guardComp) &&
            guardComp!.CraftInProgress)
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-craft-in-progress"), uid, msg.Actor);
            return;
        }

        comp.SelectedBlueprintId = msg.RecipeId;
        comp.ShowingList = false;

        if (TryGetLinkedStation(uid, comp, out var station, out var stationComp))
        {
            EjectAllItems(station, stationComp!);
            stationComp!.ActiveRecipeId = msg.RecipeId;

            if (_proto.TryIndex<ConsoleCraftPrototype>(msg.RecipeId, out var recipe))
                RollRandomItems(stationComp, recipe, comp);
        }

        RefreshConsoleState(uid, comp);
    }

    private void OnBack(EntityUid uid, ConsoleCraftConsoleComponent comp, ConsoleCraftBackMessage msg)
    {
        comp.ShowingList = true;
        RefreshConsoleState(uid, comp, showList: true);
    }

    private void OnEjectItems(EntityUid uid, ConsoleCraftConsoleComponent comp, ConsoleCraftEjectMessage msg)
    {
        if (!TryGetLinkedStation(uid, comp, out var station, out var stationComp))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-no-station"), uid, msg.Actor);
            return;
        }

        if (stationComp!.CraftInProgress)
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-craft-in-progress"), uid, msg.Actor);
            return;
        }

        EjectAllItems(station, stationComp);

        stationComp.ChosenRandomItems.Clear();

        if (stationComp.ActiveRecipeId != null &&
            _proto.TryIndex<ConsoleCraftPrototype>(stationComp.ActiveRecipeId, out var recipe))
        {
            RollRandomItems(stationComp, recipe, comp);
        }

        _popup.PopupEntity(Loc.GetString("consolecraft-items-ejected"), uid, msg.Actor);
        RefreshConsoleState(uid, comp);
    }

    private static IEnumerable<string> GetLeafPaths(DataNode node, string prefix = "")
    {
        if (node is MappingDataNode map)
        {
            foreach (var key in map.Keys)
            {
                var path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                foreach (var leaf in GetLeafPaths(map[key], path))
                    yield return leaf;
            }
        }
        else
        {
            yield return prefix;
        }
    }
    private bool HasConflictingModules(ConsoleCraftStationComponent stationComp)
    {
        var componentPathUsage = new Dictionary<string, HashSet<string>>();

        foreach (var moduleId in stationComp.InsertedModules.Keys)
        {
            if (!_proto.TryIndex<MinorItemModulePrototype>(moduleId, out var moduleDef))
                continue;

            foreach (var (compName, rawNode) in moduleDef.Components)
            {
                if (!componentPathUsage.TryGetValue(compName, out var usedPaths))
                {
                    usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    componentPathUsage[compName] = usedPaths;
                }

                foreach (var leafPath in GetLeafPaths(rawNode))
                {
                    if (!usedPaths.Add(leafPath))
                        return true;
                }
            }
        }

        return false;
    }

    private void RespawnModuleStartingItems(EntityUid target, MinorItemModulePrototype moduleDef, EntityCoordinates coords)
    {
        if (!moduleDef.Components.TryGetValue("ItemSlots", out var rawNode))
            return;

        if (!rawNode.TryGet("slots", out MappingDataNode? slotsNode))
            return;

        foreach (var (slotKey, slotValueNode) in slotsNode)
        {
            if (slotValueNode is not MappingDataNode slotMap)
                continue;

            if (!slotMap.TryGet("startingItem", out var startingItemNode))
                continue;

            var protoId = startingItemNode.ToString()?.Trim();
            if (string.IsNullOrEmpty(protoId))
                continue;

            if (!_itemSlots.TryGetSlot(target, slotKey, out var slot))
                continue;

            // Удаляем старый предмет в слоте
            if (slot.Item != null)
            {
                _itemSlots.TryEject(target, slot, null, out var ejected, true);
                if (ejected.HasValue)
                    EntityManager.DeleteEntity(ejected.Value);
            }

            if (!_proto.HasIndex<EntityPrototype>(protoId))
            {
                Log.Warning($"ConsoleCraft: startingItem '{protoId}' from module not found.");
                continue;
            }

            var newItem = EntityManager.SpawnEntity(protoId, coords);
            _itemSlots.TryInsert(target, slot, newItem, null);
        }
    }
    private void RollRandomItems(ConsoleCraftStationComponent stationComp, ConsoleCraftPrototype recipe, ConsoleCraftConsoleComponent consoleComp)
    {
        if (!consoleComp.SavedRandomChoices.TryGetValue(recipe.ID, out var saved))
        {
            saved = new Dictionary<int, string>();
            consoleComp.SavedRandomChoices[recipe.ID] = saved;
        }

        stationComp.ChosenRandomItems.Clear();

        for (var i = 0; i < recipe.RandomRequestItems.Count; i++)
        {
            var group = recipe.RandomRequestItems[i];
            if (group.Items.Count == 0)
                continue;

            if (!saved.TryGetValue(i, out var chosen))
            {
                chosen = group.Items[new Random().Next(group.Items.Count)].Id;
                saved[i] = chosen;
            }

            stationComp.ChosenRandomItems[i] = chosen;
        }
    }

    private void OnStartCraft(EntityUid uid, ConsoleCraftConsoleComponent comp, ConsoleCraftStartMessage msg)
    {
        if (!TryGetLinkedStation(uid, comp, out var station, out var stationComp))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-no-station"), uid, msg.Actor);
            return;
        }

        if (!Transform(station).Anchored)
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-station-not-anchored"), uid, msg.Actor);
            return;
        }

        if (!this.IsPowered(station, EntityManager))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-no-power"), uid, msg.Actor);
            return;
        }

        if (stationComp!.CraftInProgress)
            return;

        if (comp.SelectedBlueprintId == null ||
            !_proto.TryIndex<ConsoleCraftPrototype>(comp.SelectedBlueprintId, out var recipe))
            return;

        if (!TryComp<ConsoleCraftBlueprintReceiverComponent>(uid, out var receiver) ||
            !Blueprints.GetAvailableRecipeIds((uid, receiver)).Contains(recipe.ID))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-blueprint-not-loaded"), uid, msg.Actor);
            return;
        }

        if (!AllRequiredSatisfied(stationComp, recipe))
        {
            _popup.PopupEntity(Loc.GetString("consolecraft-missing-items"), uid, msg.Actor);
            return;
        }

        if (HasConflictingModules(stationComp!))
        {
            EjectAllItems(station, stationComp!);
            _popup.PopupEntity(Loc.GetString("consolecraft-conflicting-modules"), uid, msg.Actor);
            RefreshConsoleState(uid, comp);
            return;
        }

        stationComp.CraftInProgress = true;
        _appearance.SetData(station, ConsoleCraftStationVisuals.Working, ConsoleCraftStationVisualState.Crafting);
        stationComp.PackEndTime = _timing.CurTime + TimeSpan.FromSeconds(recipe.Time);
        StartCraftingSound(station, stationComp);
        RefreshConsoleState(uid, comp);
    }

    private void FinishCraft(EntityUid stationUid, ConsoleCraftStationComponent stationComp)
    {
        stationComp.CraftInProgress = false;
        StopCraftingSound(stationUid, stationComp);
        _appearance.SetData(stationUid, ConsoleCraftStationVisuals.Working, ConsoleCraftStationVisualState.Idle);

        if (stationComp.ActiveRecipeId == null ||
            !_proto.TryIndex<ConsoleCraftPrototype>(stationComp.ActiveRecipeId, out var recipe))
            return;

        foreach (var (_, entities) in stationComp.InsertedRequired)
            foreach (var ent in entities)
            {
                _container.Remove(ent, stationComp.ItemContainer, reparent: false, force: true);
                EntityManager.DeleteEntity(ent);
            }
        stationComp.InsertedRequired.Clear();

        foreach (var (_, entities) in stationComp.InsertedRandomRequired)
            foreach (var ent in entities)
            {
                _container.Remove(ent, stationComp.ItemContainer, reparent: false, force: true);
                EntityManager.DeleteEntity(ent);
            }
        stationComp.InsertedRandomRequired.Clear();
        stationComp.ChosenRandomItems.Clear();

        if (stationComp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(stationComp.LinkedConsole.Value, out var consoleCompRestore) &&
            consoleCompRestore.SavedRandomChoices.TryGetValue(recipe.ID, out var saved))
        {
            foreach (var (idx, proto) in saved)
                stationComp.ChosenRandomItems[idx] = proto;
        }

        var activeModules = stationComp.InsertedModules.Keys.ToList();

        foreach (var (_, modEnt) in stationComp.InsertedModules)
        {
            _container.Remove(modEnt, stationComp.ItemContainer, reparent: false, force: true);
            EntityManager.DeleteEntity(modEnt);
        }
        stationComp.InsertedModules.Clear();

        var coords  = Transform(stationUid).Coordinates;
        var crafted = EntityManager.SpawnEntity(recipe.Item, coords);

        foreach (var moduleId in activeModules)
        {
            if (!_proto.TryIndex<MinorItemModulePrototype>(moduleId, out var moduleDef))
                continue;

            var moduleTarget = crafted;

            if (moduleDef.IfModSuit.HasValue)
            {
                if (_container.TryGetContainer(crafted, "toggleable-clothing", out var toggleContainer)
                    && toggleContainer is ContainerSlot toggleSlot
                    && toggleSlot.ContainedEntity.HasValue)
                {
                    var slotEnt  = toggleSlot.ContainedEntity.Value;
                    var slotProto = MetaData(slotEnt).EntityPrototype?.ID;

                    if (slotProto == moduleDef.IfModSuit.Value.Id)
                        moduleTarget = slotEnt;
                }
            }

            foreach (var (compName, rawNode) in moduleDef.Components)
            {
                var compType = EntityManager.ComponentFactory.GetRegistration(compName).Type;

            if (compName == "ToggleableClothing")
            {
                var mappedRaw = (MappingDataNode) rawNode;

                if (mappedRaw.Has("clothingPrototype"))
                {
                    const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                    if (EntityManager.TryGetComponent(moduleTarget, compType, out var existingComp))
                    {
                        FieldInfo? actionField = null;
                        foreach (var name in new[] { "ActionEntity", "_actionEntity", "action", "Action" })
                        {
                            var f = compType.GetField(name, bf);
                            if (f != null && (f.FieldType == typeof(EntityUid?) || f.FieldType == typeof(EntityUid)))
                            {
                                actionField = f;
                                break;
                            }
                        }

                        if (actionField != null)
                        {
                            var raw = actionField.GetValue(existingComp);
                            EntityUid? actionEnt = null;
                            if (raw is EntityUid eu)
                                actionEnt = eu;
                            else if (raw != null && raw.GetType() == typeof(EntityUid?))
                                actionEnt = (EntityUid?) raw;

                            if (actionEnt.HasValue && EntityManager.EntityExists(actionEnt.Value))
                            {
                                if (actionField.FieldType == typeof(EntityUid?))
                                    actionField.SetValue(existingComp, (EntityUid?) null);

                                EntityManager.DeleteEntity(actionEnt.Value);
                            }
                        }
                    }

                    if (_container.TryGetContainer(moduleTarget, "toggleable-clothing", out var tcContainer))
                    {
                        foreach (var ent in tcContainer.ContainedEntities.ToArray())
                        {
                            _container.Remove(ent, tcContainer, reparent: false, force: true);
                            EntityManager.DeleteEntity(ent);
                        }
                    }

                    EntityManager.RemoveComponent(moduleTarget, compType);
                    var newToggle = (Component) _serialization.Read(compType, mappedRaw, skipHook: true)!;
                    EntityManager.AddComponent(moduleTarget, newToggle);
                }
                else
                {
                    if (EntityManager.TryGetComponent(moduleTarget, compType, out var existingToggle))
                        PatchComponentFields((Component) existingToggle, compType, mappedRaw);
                }

                continue;
            }

            if (compName == "ContainerContainer")
            {
                if (rawNode is MappingDataNode containerRaw &&
                    containerRaw.TryGet("containers", out MappingDataNode? containersNode))
                {
                    foreach (var (containerKey, containerNode) in containersNode)
                    {
                        if (containerNode.Tag == "!type:ContainerSlot")
                            _container.EnsureContainer<ContainerSlot>(moduleTarget, containerKey);
                        else
                            _container.EnsureContainer<Container>(moduleTarget, containerKey);
                    }
                }
                continue;
            }

                if (EntityManager.TryGetComponent(moduleTarget, compType, out var existing))
                {
                    MappingDataNode existingMapping;
                    try
                    {
                        existingMapping = (MappingDataNode) _serialization.WriteValue(
                            compType, existing, alwaysWrite: true);
                    }
                    catch (InvalidOperationException)
                    {
                        if (EntityManager.TryGetComponent(moduleTarget, compType, out var existingForPatch))
                            PatchComponentFields((Component) existingForPatch, compType, rawNode);
                        continue;
                    }

                    DeepMerge(existingMapping, rawNode);

                    var patched = (Component) _serialization.Read(compType, existingMapping, skipHook: true)!;
                    EntityManager.RemoveComponent(moduleTarget, compType);
                    EntityManager.AddComponent(moduleTarget, patched);
                }
                else
                {
                    var newComp = (Component) _serialization.Read(compType, rawNode, skipHook: true)!;
                    EntityManager.AddComponent(moduleTarget, newComp);
                }
            }

            RespawnModuleStartingItems(moduleTarget, moduleDef, coords);
        }

        if (activeModules.Count > 0)
        {
            var modulesForCrafted = new List<string>();
            var modulesForSuit    = new Dictionary<EntityUid, List<string>>();

            foreach (var moduleId in activeModules)
            {
                if (!_proto.TryIndex<MinorItemModulePrototype>(moduleId, out var md) || !md.IfModSuit.HasValue)
                {
                    modulesForCrafted.Add(moduleId);
                    continue;
                }

                if (_container.TryGetContainer(crafted, "toggleable-clothing", out var tc)
                    && tc is ContainerSlot ts
                    && ts.ContainedEntity.HasValue
                    && MetaData(ts.ContainedEntity.Value).EntityPrototype?.ID == md.IfModSuit.Value.Id)
                {
                    var suitEnt = ts.ContainedEntity.Value;
                    if (!modulesForSuit.ContainsKey(suitEnt))
                        modulesForSuit[suitEnt] = new List<string>();
                    modulesForSuit[suitEnt].Add(moduleId);
                }
                else
                {
                    modulesForCrafted.Add(moduleId);
                }
            }

            if (modulesForCrafted.Count > 0)
            {
                var modulesComp = EnsureComp<CraftedItemModulesComponent>(crafted);
                modulesComp.AppliedModules = modulesForCrafted;
            }

            foreach (var (suitEnt, suitModuleIds) in modulesForSuit)
            {
                var suitModules = EnsureComp<CraftedItemModulesComponent>(suitEnt);
                suitModules.AppliedModules = suitModuleIds;
            }
        }

        if (stationComp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftBlueprintReceiverComponent>(stationComp.LinkedConsole.Value, out var bpReceiver))
        {
            Blueprints.RecordCraftUse((stationComp.LinkedConsole.Value, bpReceiver), recipe.ID);
        }

        _popup.PopupEntity(Loc.GetString("consolecraft-craft-success"), stationUid);

        if (stationComp.LinkedConsole.HasValue &&
            TryComp<ConsoleCraftConsoleComponent>(stationComp.LinkedConsole.Value, out var consoleComp))
        {
            RefreshConsoleState(stationComp.LinkedConsole.Value, consoleComp);
        }
    }

    private static void DeepMerge(MappingDataNode target, MappingDataNode patch)
    {
        var keys = patch.Keys.ToList();
        foreach (var key in keys)
        {
            var patchValue = patch[key];
    
            if (patchValue is MappingDataNode patchMap && target.Has(key))
            {
                var existingValue = target[key];
                if (existingValue is MappingDataNode existingMap)
                {
                    DeepMerge(existingMap, patchMap);
                    continue;
                }
            }
    
            if (target.Has(key))
                target.Remove(key);
    
            target.Add(key, patchValue);
        }
    }
    private void PatchComponentFields(Component target, Type compType, MappingDataNode patch)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    
        var type = compType;
        while (type != null && type != typeof(object))
        {
            foreach (var field in type.GetFields(flags))
            {
                var attr = field.GetCustomAttribute<DataFieldAttribute>();
                if (attr == null) continue;
    
                var key = string.IsNullOrEmpty(attr.Tag)
                    ? char.ToLowerInvariant(field.Name[0]) + field.Name[1..]
                    : attr.Tag;
    
                TryPatchMember(field.FieldType, key, patch, v => field.SetValue(target, v));
            }
    
            foreach (var prop in type.GetProperties(flags))
            {
                if (!prop.CanWrite) continue;
                var attr = prop.GetCustomAttribute<DataFieldAttribute>();
                if (attr == null) continue;
    
                var key = string.IsNullOrEmpty(attr.Tag)
                    ? char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..]
                    : attr.Tag;
    
                TryPatchMember(prop.PropertyType, key, patch, v => prop.SetValue(target, v));
            }
    
            type = type.BaseType;
        }
    }
    private void TryPatchMember(Type memberType, string key, MappingDataNode patch, Action<object?> setter)
    {
        if (!patch.TryGet(key, out var node))
            return;
    
        try
        {
            setter(_serialization.Read(memberType, node, skipHook: true));
        }
        catch (Exception ex)
        {
            Log.Warning($"ConsoleCraft: failed to patch field '{key}': {ex.Message}");
        }
    }
    public void RefreshConsoleState(EntityUid uid, ConsoleCraftConsoleComponent comp, bool showList = false)
    {
        TryGetLinkedStation(uid, comp, out var station, out var stationComp);

        var entries = new List<ConsoleCraftBlueprintEntry>();
        if (TryComp<ConsoleCraftBlueprintReceiverComponent>(uid, out var bpReceiver))
        {
            foreach (var bp in Blueprints.GetLoadedBlueprints((uid, bpReceiver)))
            {
                if (!_proto.TryIndex<ConsoleCraftPrototype>(bp.Comp.Recipe, out var recipeDef))
                    continue;

                entries.Add(new ConsoleCraftBlueprintEntry
                {
                    RecipeId = bp.Comp.Recipe.Id,
                    Name = string.IsNullOrEmpty(recipeDef.Name) ? bp.Comp.Recipe.Id : recipeDef.Name,
                    Description = recipeDef.Description,
                    RemainingCrafts = bp.Comp.RemainingCrafts,
                });
            }
        }

        var noStation  = station == default;
        var selectedId = showList ? null : comp.SelectedBlueprintId;
        if (selectedId != null && entries.All(e => e.RecipeId != selectedId))
            selectedId = null;

        var requiredStatus  = new List<ConsoleCraftRequirementStatus>();
        var moduleStatus    = new List<ConsoleCraftModuleStatus>();
        var canCraft        = false;
        var craftInProgress = false;
        string? craftItemProtoId  = null;
        int?    remainingCrafts   = null;

        if (!showList && selectedId != null &&
            _proto.TryIndex<ConsoleCraftPrototype>(selectedId, out var recipe) &&
            stationComp != null)
        {
            craftItemProtoId = recipe.Item.Id;
            craftInProgress  = stationComp.CraftInProgress;
            remainingCrafts  = entries.FirstOrDefault(e => e.RecipeId == selectedId)?.RemainingCrafts;

            foreach (var req in recipe.RequestItems)
            {
                var inserted = stationComp.InsertedRequired.TryGetValue(req.ItemProto.Id, out var il)
                    ? il.Count : 0;
                requiredStatus.Add(new ConsoleCraftRequirementStatus
                {
                    Label    = string.IsNullOrEmpty(req.Label)
                        ? (_proto.TryIndex<EntityPrototype>(req.ItemProto.Id, out var reqProto)
                            ? reqProto.Name
                            : req.ItemProto.Id)
                        : req.Label,
                    ProtoId  = req.ItemProto.Id,
                    Required = req.Amount,
                    Inserted = inserted,
                });
            }

            for (var i = 0; i < recipe.RandomRequestItems.Count; i++)
            {
                var group = recipe.RandomRequestItems[i];
                if (group.Items.Count == 0)
                    continue;

                var insertedRandom = stationComp.InsertedRandomRequired.TryGetValue(i, out var rl)
                    ? rl.Count : 0;

                requiredStatus.Add(new ConsoleCraftRequirementStatus
                {
                    Label    = group.Label,
                    ProtoId  = stationComp.ChosenRandomItems.TryGetValue(i, out var chosen)
                        ? chosen
                        : group.Items[0].Id,
                    Required = group.Amount,
                    Inserted = insertedRandom,
                });
            }

            foreach (var moduleId in recipe.MinorItems)
            {
                if (!_proto.TryIndex<MinorItemModulePrototype>(moduleId, out var moduleDef))
                    continue;
                moduleStatus.Add(new ConsoleCraftModuleStatus
                {
                    ModuleId    = moduleId.Id,
                    Description = moduleDef.Description,
                    Inserted    = stationComp.InsertedModules.ContainsKey(moduleId.Id),
                });
            }

            canCraft = AllRequiredSatisfied(stationComp, recipe) && !craftInProgress;
        }

        _ui.SetUiState(uid, ConsoleCraftUiKey.Key, new ConsoleCraftConsoleState
        {
            AvailableRecipes   = entries,
            SelectedRecipeId   = selectedId,
            CraftItemProtoId   = craftItemProtoId,
            RequiredStatus     = requiredStatus,
            ModuleStatus       = moduleStatus,
            CanCraft           = canCraft,
            CraftInProgress    = craftInProgress,
            NoStation          = noStation,
            SelectedBlueprintRemainingCrafts = remainingCrafts,
        });
    }

    private void StartCraftingSound(EntityUid stationUid, ConsoleCraftStationComponent comp)
    {
        if (comp.CraftingSound == null)
            return;

        StopCraftingSound(stationUid, comp);

        var stream = _audio.PlayPvs(
            comp.CraftingSound,
            stationUid,
            AudioParams.Default.WithLoop(true));

        comp.CraftingSoundEntity = stream?.Entity;
    }

    private void StopCraftingSound(EntityUid stationUid, ConsoleCraftStationComponent comp)
    {
        if (!comp.CraftingSoundEntity.HasValue)
            return;

        _audio.Stop(comp.CraftingSoundEntity.Value);
        comp.CraftingSoundEntity = null;
    }

    private bool TryGetLinkedStation(EntityUid consoleUid, ConsoleCraftConsoleComponent comp, out EntityUid stationUid, out ConsoleCraftStationComponent? stationComp)
    {
        stationUid  = default;
        stationComp = null;

        if (!comp.LinkedStation.HasValue)
            return false;

        var target = comp.LinkedStation.Value;
        if (!EntityManager.EntityExists(target))
        {
            comp.LinkedStation = null;
            return false;
        }

        if (!TryComp(target, out stationComp))
            return false;

        stationUid = target;
        return true;
    }

    private static bool AllRequiredSatisfied(ConsoleCraftStationComponent station, ConsoleCraftPrototype recipe)
    {
        foreach (var req in recipe.RequestItems)
        {
            var inserted = station.InsertedRequired.TryGetValue(req.ItemProto.Id, out var list)
                ? list.Count : 0;
            if (inserted < req.Amount)
                return false;
        }

        for (var i = 0; i < recipe.RandomRequestItems.Count; i++)
        {
            var group = recipe.RandomRequestItems[i];
            if (group.Items.Count == 0)
                continue;

            var inserted = station.InsertedRandomRequired.TryGetValue(i, out var rl)
                ? rl.Count : 0;
            if (inserted < group.Amount)
                return false;
        }

        return true;
    }

    public void EjectAllItems(EntityUid stationUid, ConsoleCraftStationComponent comp)
    {
        foreach (var ent in comp.ItemContainer.ContainedEntities.ToArray())
            _container.Remove(ent, comp.ItemContainer);
        comp.InsertedRequired.Clear();
        comp.InsertedRandomRequired.Clear();
        comp.InsertedModules.Clear();
    }

    private void NotifyLinkedConsole(EntityUid stationUid, ConsoleCraftStationComponent stationComp)
    {
        if (!stationComp.LinkedConsole.HasValue)
            return;
        var consoleUid = stationComp.LinkedConsole.Value;
        if (TryComp<ConsoleCraftConsoleComponent>(consoleUid, out var consoleComp))
            RefreshConsoleState(consoleUid, consoleComp);
    }

    private bool IsMultitool(EntityUid uid) => HasComp<NetworkConfiguratorComponent>(uid);

    private InsertResult TryInsertItem(
        EntityUid stationUid,
        ConsoleCraftStationComponent stationComp,
        ConsoleCraftPrototype recipe,
        EntityUid itemUid)
    {
        if (!EntityManager.EntityExists(itemUid))
            return InsertResult.WrongItem;

        var itemProto = MetaData(itemUid).EntityPrototype?.ID;
        if (itemProto == null)
            return InsertResult.WrongItem;

        foreach (var moduleProtoId in recipe.MinorItems)
        {
            if (!_proto.TryIndex<MinorItemModulePrototype>(moduleProtoId, out var moduleDef))
                continue;

            var matchesItem = moduleDef.ModuleItem.HasValue &&
                              moduleDef.ModuleItem.Value.Id == itemProto;

            var matchesTag = !matchesItem &&
                             moduleDef.Tag != null &&
                             moduleDef.Tag.Any(t => _tag.HasTag(itemUid, t));

            if (!matchesItem && !matchesTag)
                continue;

            if (stationComp.InsertedModules.ContainsKey(moduleProtoId.Id))
                return InsertResult.ModuleDuplicate;

            if (stationComp.InsertedModules.Count >= 2)
                return InsertResult.ModuleFull;

            _container.Insert(itemUid, stationComp.ItemContainer);
            stationComp.InsertedModules[moduleProtoId.Id] = itemUid;
            return InsertResult.SuccessModule;
        }

        var requiredItem = recipe.RequestItems.FirstOrDefault(r => r.ItemProto.Id == itemProto);
        if (requiredItem != null)
        {
            var insertedCount = stationComp.InsertedRequired.TryGetValue(requiredItem.ItemProto.Id, out var list2)
                ? list2.Count : 0;
            if (insertedCount >= requiredItem.Amount)
                return InsertResult.WrongItem;

            _container.Insert(itemUid, stationComp.ItemContainer);
            if (!stationComp.InsertedRequired.ContainsKey(requiredItem.ItemProto.Id))
                stationComp.InsertedRequired[requiredItem.ItemProto.Id] = new List<EntityUid>();

            stationComp.InsertedRequired[requiredItem.ItemProto.Id].Add(itemUid);
            return InsertResult.Success;
        }

        for (var i = 0; i < recipe.RandomRequestItems.Count; i++)
        {
            var group = recipe.RandomRequestItems[i];

            if (!stationComp.ChosenRandomItems.TryGetValue(i, out var chosenProto))
                continue;

            if (chosenProto != itemProto)
                continue;

            var insertedCount = stationComp.InsertedRandomRequired.TryGetValue(i, out var rl)
                ? rl.Count : 0;
            if (insertedCount >= group.Amount)
                continue;

            _container.Insert(itemUid, stationComp.ItemContainer);
            if (!stationComp.InsertedRandomRequired.ContainsKey(i))
                stationComp.InsertedRandomRequired[i] = new List<EntityUid>();

            stationComp.InsertedRandomRequired[i].Add(itemUid);
            return InsertResult.Success;
        }

        return InsertResult.WrongItem;
    }
}

public enum InsertResult
{
    Success,
    SuccessModule,
    ModuleFull,
    ModuleDuplicate,
    WrongItem,
}