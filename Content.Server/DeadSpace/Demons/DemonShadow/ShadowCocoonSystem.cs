// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Abilities.Cocoon.Components;
using Content.Server.DeadSpace.Demons.DemonShadow.Components;
using Content.Shared.Body.Events;
using Content.Shared.Destructible;
using Content.Shared.Eye;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Shared.Gibbing;

namespace Content.Server.DeadSpace.Demons.LockCocoon;

public sealed class ShadowCocoonSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLightSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCocoonComponent, BeingGibbedEvent>(OnGibbed);
        SubscribeLocalEvent<ShadowCocoonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowCocoonComponent, ComponentShutdown>(OnShutDown);
        SubscribeLocalEvent<ShadowCocoonComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowCocoonComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<ShadowCocoonComponent, DestructionEventArgs>(OnDestruction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shadowCocoonComponent = EntityQueryEnumerator<ShadowCocoonComponent>();
        while (shadowCocoonComponent.MoveNext(out var uid, out var component))
        {
            if (_gameTiming.CurTime > component.NextTick)
            {
                TurnOffElectricity(uid, component);
            }
        }
    }

    private void TurnOffElectricity(EntityUid uid, ShadowCocoonComponent component)
    {
        var xform = Transform(uid);

        var entities = _lookup.GetEntitiesInRange<PointLightComponent>(_transform.GetMapCoordinates(uid, xform), component.Range);
        List<EntityUid> lights = new List<EntityUid>();

        foreach (var (entity, pointLightComp) in entities)
        {
            if (CheckParentVisibilityLayer(entity))
                continue;

            lights.Add(entity);
            _pointLightSystem.SetEnabled(entity, false);
            component.PointEntities.Add(entity);
        }

        foreach (var entity in component.PointEntities)
        {
            if (!lights.Contains(entity))
            {
                if (TryComp<PointLightComponent>(entity, out var poweredLight))
                {
                    _pointLightSystem.SetEnabled(entity, true);
                }
            }
        }

        component.NextTick = _gameTiming.CurTime + TimeSpan.FromSeconds(1);
    }

    /// <summary>
    /// Данный метод удаляет сущность из контейнера, в который ее вставили.
    /// Необходимо для удаления возможности перемещать теневые коконы в ящиках/шкафах.
    /// </summary>
    private void OnInserted(EntityUid uid, ShadowCocoonComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (!HasComp<EntityStorageComponent>(args.Container.Owner))
            return;

        // Откладываем удаление на следующий тик! Иначе может возникнуть проблема с флагами системы контейнеров
        Timer.Spawn(0, () =>
        {
            _popupSystem.PopupEntity(
                "Стены хранилища не могут удержать эту материю",
                args.Container.Owner,
                PopupType.MediumCaution
            );
            _containerSystem.Remove(uid, args.Container);
        });
    }

    /// <summary>  
    /// Проверяет, отличается ли слой видимости родителя от Normal  
    /// </summary>  
    public bool CheckParentVisibilityLayer(EntityUid uid)
    {
        if (!_containerSystem.TryGetContainingContainer((uid, null, null), out var container))
            return false;

        var parentUid = container.Owner;

        if (TryComp<VisibilityComponent>(parentUid, out var visibilityComponent))
        {
            return visibilityComponent.Layer != (int)VisibilityFlags.Normal;
        }

        if (HasComp<GhostComponent>(parentUid))
            return true;

        return false;
    }

    private void OnInit(EntityUid uid, ShadowCocoonComponent component, ComponentInit args)
    {
        component.NextTick = _gameTiming.CurTime + TimeSpan.FromSeconds(1);
    }

    private void OnMapInit(EntityUid uid, ShadowCocoonComponent component, MapInitEvent args)
    {
        if (!HasComp<CocoonComponent>(uid))
            AddComp<CocoonComponent>(uid);
    }

    private void OnGibbed(EntityUid uid, ShadowCocoonComponent component, BeingGibbedEvent args)
    {
        DestroyCocoon(uid, component);
    }

    private void OnShutDown(EntityUid uid, ShadowCocoonComponent component, ComponentShutdown args)
    {
        DestroyCocoon(uid, component);
    }

    private void OnDestruction(EntityUid uid, ShadowCocoonComponent component, DestructionEventArgs args)
    {
        DestroyCocoon(uid, component);
    }

    private void DestroyCocoon(EntityUid uid, ShadowCocoonComponent component)
    {
        foreach (var entity in component.PointEntities)
        {
            if (TryComp<PointLightComponent>(entity, out _))
            {
                _pointLightSystem.SetEnabled(entity, true);
            }
        }
    }
}
