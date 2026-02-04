using System.Linq;
using System.Numerics;
using Content.Shared._Frostheim.Shuttle;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client._Frostheim.Shuttle;

public sealed class NavGridControl : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public event Action<int, int>? OnCellClick;

    private static readonly Color BgColor = Color.FromHex("#0a0e14");
    private static readonly Color GridLineColor = Color.FromHex("#1a2a3a");
    private static readonly Color EmptyColor = Color.FromHex("#0f1520");
    private static readonly Color AsteroidColor = Color.FromHex("#8b2020");
    private static readonly Color AsteroidDarkColor = Color.FromHex("#5a1515");
    private static readonly Color StartColor = Color.FromHex("#4a8a4a");
    private static readonly Color EndColor = Color.FromHex("#c4a030");
    private static readonly Color PathColor = Color.FromHex("#2a6a4a");
    private static readonly Color PathHeadColor = Color.FromHex("#3a9a6a");
    private static readonly Color HoverColor = Color.FromHex("#3a5a7a");
    private static readonly Color TextColor = Color.FromHex("#7a9ab0");
    private static readonly Color TextDimColor = Color.FromHex("#3a4a5a");

    private NavGridCell[]? _grid;
    private int _gridWidth;
    private int _gridHeight;
    private int _startX, _startY;
    private int _endX, _endY;
    private List<(int X, int Y)> _path = new();
    private (int X, int Y)? _hovered;
    private Font _font;

    private const float CellPadding = 1f;
    private const float HeaderSize = 20f;

    public NavGridControl()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;
        HorizontalExpand = true;
        VerticalExpand = true;

        var fontRes = _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf");
        _font = new VectorFont(fontRes, 8);
    }

    public void UpdateState(FrostheimNavConsoleState state)
    {
        _grid = state.Grid;
        _gridWidth = state.GridWidth;
        _gridHeight = state.GridHeight;
        _startX = state.StartX;
        _startY = state.StartY;
        _endX = state.EndX;
        _endY = state.EndY;
        _path = state.CurrentPath;
    }

    private float CellSize
    {
        get
        {
            if (_gridWidth == 0 || _gridHeight == 0) return 0;
            var availW = PixelWidth - HeaderSize * UIScale;
            var availH = PixelHeight - HeaderSize * UIScale;
            return MathF.Min(availW / _gridWidth, availH / _gridHeight);
        }
    }

    private Vector2 GridOrigin
    {
        get
        {
            var cellSize = CellSize;
            var totalW = _gridWidth * cellSize;
            var totalH = _gridHeight * cellSize;
            var offsetX = (PixelWidth - totalW) / 2f;
            var offsetY = HeaderSize * UIScale;
            return new Vector2(MathF.Max(offsetX, HeaderSize * UIScale), offsetY);
        }
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (_grid == null || _gridWidth == 0 || _gridHeight == 0)
            return;

        handle.DrawRect(PixelSizeBox, BgColor);

        var cellSize = CellSize;
        var origin = GridOrigin;

        DrawColumnHeaders(handle, origin, cellSize);
        DrawRowHeaders(handle, origin, cellSize);
        DrawCells(handle, origin, cellSize);
        DrawGridLines(handle, origin, cellSize);
        DrawHover(handle, origin, cellSize);
    }

    private void DrawColumnHeaders(DrawingHandleScreen handle, Vector2 origin, float cellSize)
    {
        for (var x = 0; x < _gridWidth; x++)
        {
            var text = x.ToString("X");
            var pos = new Vector2(origin.X + x * cellSize + cellSize * 0.35f, 2f * UIScale);
            handle.DrawString(_font, pos, text, UIScale, TextDimColor);
        }
    }

    private void DrawRowHeaders(DrawingHandleScreen handle, Vector2 origin, float cellSize)
    {
        for (var y = 0; y < _gridHeight; y++)
        {
            var text = y.ToString("X");
            var pos = new Vector2(2f * UIScale, origin.Y + y * cellSize + cellSize * 0.25f);
            handle.DrawString(_font, pos, text, UIScale, TextDimColor);
        }
    }

    private void DrawCells(DrawingHandleScreen handle, Vector2 origin, float cellSize)
    {
        var pad = CellPadding * UIScale;

        for (var y = 0; y < _gridHeight; y++)
        {
            for (var x = 0; x < _gridWidth; x++)
            {
                var rect = new UIBox2(
                    origin.X + x * cellSize + pad,
                    origin.Y + y * cellSize + pad,
                    origin.X + (x + 1) * cellSize - pad,
                    origin.Y + (y + 1) * cellSize - pad);

                Color color;

                if (x == _startX && y == _startY)
                    color = StartColor;
                else if (x == _endX && y == _endY)
                    color = EndColor;
                else if (IsOnPath(x, y))
                    color = IsPathHead(x, y) ? PathHeadColor : PathColor;
                else if (_grid![y * _gridWidth + x] == NavGridCell.Asteroid)
                    color = (x + y) % 2 == 0 ? AsteroidColor : AsteroidDarkColor;
                else
                    color = EmptyColor;

                handle.DrawRect(rect, color);

                if (x == _startX && y == _startY)
                {
                    var textPos = new Vector2(rect.Left + cellSize * 0.2f, rect.Top + cellSize * 0.1f);
                    handle.DrawString(_font, textPos, "SRC", UIScale, BgColor);
                }
                else if (x == _endX && y == _endY)
                {
                    var textPos = new Vector2(rect.Left + cellSize * 0.2f, rect.Top + cellSize * 0.1f);
                    handle.DrawString(_font, textPos, "DST", UIScale, BgColor);
                }
            }
        }
    }

    private void DrawGridLines(DrawingHandleScreen handle, Vector2 origin, float cellSize)
    {
        var totalW = _gridWidth * cellSize;
        var totalH = _gridHeight * cellSize;

        for (var x = 0; x <= _gridWidth; x++)
        {
            var xPos = origin.X + x * cellSize;
            handle.DrawLine(new Vector2(xPos, origin.Y), new Vector2(xPos, origin.Y + totalH), GridLineColor);
        }

        for (var y = 0; y <= _gridHeight; y++)
        {
            var yPos = origin.Y + y * cellSize;
            handle.DrawLine(new Vector2(origin.X, yPos), new Vector2(origin.X + totalW, yPos), GridLineColor);
        }
    }

    private void DrawHover(DrawingHandleScreen handle, Vector2 origin, float cellSize)
    {
        if (_hovered == null)
            return;

        var (hx, hy) = _hovered.Value;
        if (hx < 0 || hx >= _gridWidth || hy < 0 || hy >= _gridHeight)
            return;

        if (_grid![hy * _gridWidth + hx] == NavGridCell.Asteroid)
            return;

        var pad = CellPadding * UIScale;
        var rect = new UIBox2(
            origin.X + hx * cellSize + pad,
            origin.Y + hy * cellSize + pad,
            origin.X + (hx + 1) * cellSize - pad,
            origin.Y + (hy + 1) * cellSize - pad);

        handle.DrawRect(rect, HoverColor.WithAlpha(0.3f));
    }

    private bool IsOnPath(int x, int y) => _path.Any(p => p.X == x && p.Y == y);
    private bool IsPathHead(int x, int y) => _path.Count > 0 && _path[^1].X == x && _path[^1].Y == y;

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);
        _hovered = ScreenToCell(args.RelativePosition);
    }

    protected override void MouseExited()
    {
        base.MouseExited();
        _hovered = null;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        var cell = ScreenToCell(args.RelativePosition);
        if (cell != null)
        {
            OnCellClick?.Invoke(cell.Value.X, cell.Value.Y);
            args.Handle();
        }
    }

    private (int X, int Y)? ScreenToCell(Vector2 pos)
    {
        var origin = GridOrigin;
        var cellSize = CellSize;

        var relX = pos.X * UIScale - origin.X;
        var relY = pos.Y * UIScale - origin.Y;

        if (relX < 0 || relY < 0)
            return null;

        var cx = (int)(relX / cellSize);
        var cy = (int)(relY / cellSize);

        if (cx < 0 || cx >= _gridWidth || cy < 0 || cy >= _gridHeight)
            return null;

        return (cx, cy);
    }
}
