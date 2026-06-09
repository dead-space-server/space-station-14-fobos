using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Shared.Maps;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class NinjaSmokeAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NinjaSmokeAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NinjaSmokeAbilityComponent, GetItemActionsEvent>(OnGetActions);

        SubscribeLocalEvent<NinjaSmokeAbilityComponent, NinjaSmokeAbilityActionEvent>(OnSmokeAction);
        SubscribeLocalEvent<NinjaSmokeAbilityComponent, NinjaToggleAutoSmokeActionEvent>(OnSmokeAutoModeToggleAction);
    }

    private void OnMapInit(Entity<NinjaSmokeAbilityComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ActionSmokeEntity, comp.ActionSmoke);
        _actions.AddAction(uid, ref comp.ActionAutoSmokeEntity, comp.ActionAutoSmoke);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<NinjaSmokeAbilityComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;
        args.AddAction(ent.Comp.ActionSmokeEntity);
        args.AddAction(ent.Comp.ActionAutoSmokeEntity);
    }

    private void OnSmokeAction(Entity<NinjaSmokeAbilityComponent> ent, ref NinjaSmokeAbilityActionEvent args)
    {
        args.Handled = true;
        SpawnNinjaSmoke(ent, false);
    }

    private void OnSmokeAutoModeToggleAction(Entity<NinjaSmokeAbilityComponent> ent, ref NinjaToggleAutoSmokeActionEvent args)
    {
        args.Handled = true;
        var (uid, comp) = ent;
        comp.AutoMode = !comp.AutoMode;
        Dirty(uid, comp);
        _actions.SetToggled(comp.ActionAutoSmokeEntity, comp.AutoMode);
    }

    public void SpawnNinjaSmoke(Entity<NinjaSmokeAbilityComponent> ent, bool AutoMode)
    {
        var xform = Transform(ent);
        var mapCoords = _xform.GetMapCoordinates(ent);
        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid) ||
            !_map.TryGetTileRef(gridUid, grid, xform.Coordinates, out var tileRef))
            return;

        if (_spreader.RequiresFloorToSpread(ent.Comp.SmokePrototype.ToString()) && _turf.IsSpace(tileRef))
            return;

        var coords = _map.MapToGrid(gridUid, mapCoords);
        var smoke = Spawn(ent.Comp.SmokePrototype, coords.SnapToGrid());
        if (!TryComp<SmokeComponent>(smoke, out var smokeComp))
        {
            Logger.ErrorS("ninja-smoke", $"Smoke prototype {ent.Comp.SmokePrototype} was missing SmokeComponent");
            return;
        }

        _audio.PlayPvs(ent.Comp.SmokeSound, ent);
        if (!AutoMode)
        {
            _smoke.StartSmoke(smoke, new Solution(), (float)ent.Comp.Duration.TotalSeconds, ent.Comp.SpreadAmount, smokeComp);
        }
        else if (ent.Comp.AutoMode)
        {
            _smoke.StartSmoke(smoke, new Solution(), (float)ent.Comp.DurationAutoMode.TotalSeconds, ent.Comp.SpreadAmountAutoMode, smokeComp);
        }
    }
}