using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared._Frostheim.Campfire;
using Content.Shared._Frostheim.HeatSource;
using Content.Shared.Audio;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.Temperature;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Server._Frostheim.Campfire;

public sealed class FrostCampfireSystem : EntitySystem
{
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostCampfireComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<FrostCampfireComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FrostCampfireComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Lit)
                continue;

            comp.BurnTimeRemaining -= frameTime;

            if (comp.BurnTimeRemaining <= 0)
            {
                comp.BurnTimeRemaining = 0;
                Extinguish(uid, comp);
            }
        }
    }

    private void OnInit(EntityUid uid, FrostCampfireComponent comp, ComponentInit args)
    {
        UpdateVisuals(uid, comp);
    }

    private void OnInteractUsing(EntityUid uid, FrostCampfireComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<StackComponent>(args.Used, out var stack) && stack.StackTypeId == "WoodPlank")
        {
            if (comp.BurnTimeRemaining >= comp.MaxBurnTime)
            {
                _popup.PopupEntity(Loc.GetString("frostheim-campfire-full"), uid, args.User);
                return;
            }

            comp.BurnTimeRemaining = Math.Min(comp.BurnTimeRemaining + comp.FuelPerPlank, comp.MaxBurnTime);
            _stack.SetCount(args.Used, stack.Count - 1, stack);
            _popup.PopupEntity(Loc.GetString("frostheim-campfire-fueled"), uid, args.User);
            Dirty(uid, comp);
            args.Handled = true;
            return;
        }

        if (!comp.Lit && comp.BurnTimeRemaining > 0)
        {
            var hotEvent = new IsHotEvent();
            RaiseLocalEvent(args.Used, hotEvent);

            if (hotEvent.IsHot)
            {
                Ignite(uid, comp);
                _popup.PopupEntity(Loc.GetString("frostheim-campfire-lit"), uid, args.User);
                args.Handled = true;
            }
        }
    }

    private void Ignite(EntityUid uid, FrostCampfireComponent comp)
    {
        comp.Lit = true;
        Dirty(uid, comp);

        if (TryComp<HeatSourceComponent>(uid, out var heat))
        {
            heat.Enabled = true;
            Dirty(uid, heat);
        }

        _pointLight.SetEnabled(uid, true);
        _ambientSound.SetAmbience(uid, true);
        UpdateVisuals(uid, comp);
    }

    private void Extinguish(EntityUid uid, FrostCampfireComponent comp)
    {
        comp.Lit = false;
        Dirty(uid, comp);

        if (TryComp<HeatSourceComponent>(uid, out var heat))
        {
            heat.Enabled = false;
            Dirty(uid, heat);
        }

        _pointLight.SetEnabled(uid, false);
        _ambientSound.SetAmbience(uid, false);
        UpdateVisuals(uid, comp);
    }

    private void UpdateVisuals(EntityUid uid, FrostCampfireComponent comp)
    {
        _appearance.SetData(uid, FrostCampfireVisualLayers.Fire, comp.Lit);
    }
}
