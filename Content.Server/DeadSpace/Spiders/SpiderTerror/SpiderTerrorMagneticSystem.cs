// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Spiders.SpiderTerror.Components;
using Content.Shared.Gravity;
using Content.Shared.Maps;
using Content.Shared.Spider;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.DeadSpace.Spiders.SpiderTerror;

public sealed class SpiderTerrorMagneticSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;

    private ushort _vebTileId;
    private EntityQuery<SpiderWebObjectComponent> _webQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderTerrorComponent, IsWeightlessEvent>(OnIsWeightless);
        SubscribeLocalEvent<SpiderTerrorComponent, MoveEvent>(OnMove);

        _webQuery = GetEntityQuery<SpiderWebObjectComponent>();
    }

    private ushort GetVebTileId()
    {
        if (_vebTileId == 0)
        {
            var tileDef = (ContentTileDefinition)_tileDef["VebTile"];
            _vebTileId = tileDef.TileId;
        }

        return _vebTileId;
    }

    private void OnMove(Entity<SpiderTerrorComponent> ent, ref MoveEvent args)
    {
        if (!TryComp<GravityAffectedComponent>(ent.Owner, out var gravity))
            return;

        _gravity.RefreshWeightless((ent.Owner, gravity));
    }

    private void OnIsWeightless(Entity<SpiderTerrorComponent> ent, ref IsWeightlessEvent args)
    {
        if (args.Handled)
            return;

        if (IsOnWeb(ent.Owner))
        {
            args.IsWeightless = false;
            args.Handled = true;
        }
    }

    private bool IsOnWeb(EntityUid uid)
    {
        var xform = Transform(uid);

        if (xform.GridUid == null)
            return false;

        if (!TryComp<MapGridComponent>(xform.GridUid.Value, out var grid))
            return false;

        var tile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        if (tile.Tile.TypeId == GetVebTileId())
            return true;

        foreach (var ent in _map.GetAnchoredEntities(xform.GridUid.Value, grid, xform.Coordinates))
        {
            if (_webQuery.HasComp(ent))
                return true;
        }

        return false;
    }
}