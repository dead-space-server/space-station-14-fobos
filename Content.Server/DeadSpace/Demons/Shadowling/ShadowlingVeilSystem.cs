// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Power.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Light.Components;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Content.Shared.Damage.Systems;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingVeilSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    private const string ActionVeilId = "ActionShadowlingVeil";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingVeilComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowlingVeilComponent, ShadowlingVeilActionEvent>(OnShadowlingVeil);
    }

    private void OnMapInit(EntityUid uid, ShadowlingVeilComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ActionVeilId);
    }

    private void OnShadowlingVeil(EntityUid uid, ShadowlingVeilComponent component, ShadowlingVeilActionEvent args)
    {
        if (args.Handled || component.VeilActive) return;

        var worldPos = _transform.GetMapCoordinates(uid);
        var entities = _lookup.GetEntitiesInRange(worldPos, 10f);
        component.AffectedLights.Clear();

        foreach (var entity in entities)
        {
            if (_container.TryGetContainer(entity, "light_bulb", out var container))
            {
                if (container.ContainedEntities.Count > 0)
                {
                    bool hasIntactBulb = false;
                    foreach (var bulbUid in container.ContainedEntities)
                    {
                        if (TryComp<LightBulbComponent>(bulbUid, out var bulb) && bulb.State == LightBulbState.Normal)
                        {
                            hasIntactBulb = true;
                            break;
                        }
                    }

                    if (hasIntactBulb)
                    {
                        var damage = new DamageSpecifier();
                        damage.DamageDict.Add("Blunt", 20);
                        _damageable.TryChangeDamage(entity, damage, true);
                        Spawn("EffectSparks", Transform(entity).Coordinates);
                    }
                }
                continue;
            }

            if (TryComp<PointLightComponent>(entity, out var light) && light.Enabled)
            {
                _light.SetEnabled(entity, false, light);
                component.AffectedLights.Add(entity);
            }
        }

        component.VeilActive = true;
        component.VeilTimer = 10f;
        _popup.PopupEntity("Тьма поглощает свет!", uid, uid, PopupType.Large);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ShadowlingVeilComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.VeilActive) continue;

            comp.VeilTimer -= frameTime;

            var worldPos = _transform.GetMapCoordinates(uid);
            var entities = _lookup.GetEntitiesInRange<PointLightComponent>(worldPos, 10f);

            foreach (var entity in entities)
            {
                if (comp.AffectedLights.Contains(entity))
                    continue;

                if (_container.TryGetContainer(entity, "light_bulb", out _))
                    continue;

                if (TryComp<PointLightComponent>(entity, out var light) && light.Enabled)
                {
                    _light.SetEnabled(entity, false, light);
                    comp.AffectedLights.Add(entity);
                }
            }

            foreach (var lightUid in comp.AffectedLights)
            {
                if (TryComp<PointLightComponent>(lightUid, out var light) && light.Enabled)
                    _light.SetEnabled(lightUid, false, light);
            }

            if (comp.VeilTimer <= 0)
            {
                foreach (var lightUid in comp.AffectedLights)
                {
                    if (!Exists(lightUid)) continue;

                    bool canLight = true;
                    if (TryComp<ApcPowerReceiverComponent>(lightUid, out var power) && !power.Powered) canLight = false;
                    if (canLight && TryComp<HandheldLightComponent>(lightUid, out var handheld) && !handheld.Activated) canLight = false;
                    if (canLight && TryComp<UnpoweredFlashlightComponent>(lightUid, out var unp) && !unp.LightOn) canLight = false;

                    if (canLight) _light.SetEnabled(lightUid, true);
                }
                _popup.PopupEntity("Тьма рассеивается!", uid, uid, PopupType.Large);
                comp.AffectedLights.Clear();
                comp.VeilActive = false;
            }
        }
    }
}