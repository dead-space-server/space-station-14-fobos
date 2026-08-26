// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.DeadSpace.BSAConsole;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;

namespace Content.Client.DeadSpace.BSAConsole;

public sealed partial class BSANavMapControl : MapGridControl
{
    public Action<MapCoordinates>? OnMapClick;

    private BSARadarState? _state;

    protected override bool Draggable => true;

    public BSANavMapControl() : base(16f, 512f, 256f)
    {
    }

    public void UpdateState(BSARadarState? state)
    {
        if (_state?.MapId != state?.MapId)
        {
            Offset = Vector2.Zero;
            TargetOffset = Vector2.Zero;
        }

        _state = state;
    }

    public Vector2? WorldToScreen(MapCoordinates coordinates)
    {
        if (_state == null || (int) coordinates.MapId != _state.MapId)
            return null;

        return WorldToScreen(coordinates.Position);
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (_state == null || args.Function != EngineKeyFunctions.UIClick || OnMapClick == null)
            return;

        var localPosition = args.PointerLocation.Position - GlobalPixelPosition;
        var relativePosition = InverseMapPosition(localPosition);
        var mapPosition = new MapCoordinates(
            _state.Center + relativePosition,
            new MapId(_state.MapId));

        OnMapClick.Invoke(mapPosition);
        args.Handle();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);
        DrawBacking(handle);

        if (_state == null)
        {
            DrawNoSignal(handle);
            return;
        }

        handle.DrawCircle(MidPointVector, ScaledMinimapRadius, Color.FromHex("#2A4A2A"), false);

        foreach (var grid in _state.Grids)
        {
            var rotation = new Angle(grid.Rotation);
            var half = grid.HalfExtents;
            var worldCorners = new[]
            {
                grid.Center + rotation.RotateVec(new Vector2(-half.X, -half.Y)),
                grid.Center + rotation.RotateVec(new Vector2(half.X, -half.Y)),
                grid.Center + rotation.RotateVec(new Vector2(half.X, half.Y)),
                grid.Center + rotation.RotateVec(new Vector2(-half.X, half.Y)),
            };

            var screenCorners = new Vector2[worldCorners.Length];
            for (var i = 0; i < worldCorners.Length; i++)
                screenCorners[i] = WorldToScreen(worldCorners[i]);

            var color = grid.Selected ? Color.FromHex("#54D98C") : Color.FromHex("#4FA3C7");
            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, screenCorners, color.WithAlpha(0.18f));

            for (var i = 0; i < screenCorners.Length; i++)
                handle.DrawLine(screenCorners[i], screenCorners[(i + 1) % screenCorners.Length], color);

            var center = WorldToScreen(grid.Center);
            handle.DrawRect(new UIBox2(center.X - 2f, center.Y - 2f, center.X + 2f, center.Y + 2f), color);
        }
    }

    private Vector2 WorldToScreen(Vector2 worldPosition)
    {
        var relative = worldPosition - _state!.Center - Offset;
        return ScalePosition(new Vector2(relative.X, -relative.Y));
    }
}
