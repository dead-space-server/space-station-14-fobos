using Content.Shared._Frostheim.Shuttle;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._Frostheim.Shuttle;

public sealed class FrostheimNavConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FrostheimShuttleSystem _shuttleSystem = default!;

    private const int GridWidth = 12;
    private const int GridHeight = 12;
    private const float AsteroidChance = 0.25f;
    private const float CalibrationTolerance = 0.05f;
    private const int CalibrationSliders = 3;

    private readonly Dictionary<EntityUid, NavGameData> _games = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<FrostheimNavConsoleComponent>(FrostheimNavConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUIOpened);
            subs.Event<BoundUIClosedEvent>(OnUIClosed);
            subs.Event<NavConsolePathClickMessage>(OnPathClick);
            subs.Event<NavConsoleResetPathMessage>(OnResetPath);
            subs.Event<NavConsoleCalibrateMessage>(OnCalibrate);
            subs.Event<NavConsoleConfirmMessage>(OnConfirm);
        });
    }

    private void OnUIOpened(EntityUid uid, FrostheimNavConsoleComponent comp, BoundUIOpenedEvent args)
    {
        if (!_games.ContainsKey(uid))
            _games[uid] = GenerateGame();

        SendState(uid);
    }

    private void OnUIClosed(EntityUid uid, FrostheimNavConsoleComponent comp, BoundUIClosedEvent args)
    {
    }

    private void OnPathClick(EntityUid uid, FrostheimNavConsoleComponent comp, NavConsolePathClickMessage args)
    {
        if (!_games.TryGetValue(uid, out var game) || game.Phase != NavGamePhase.Pathfinding)
            return;

        var x = args.X;
        var y = args.Y;

        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            return;

        if (game.Grid[y * GridWidth + x] == NavGridCell.Asteroid)
            return;

        if (x == game.StartX && y == game.StartY)
        {
            game.Path.Clear();
            game.Path.Add((game.StartX, game.StartY));
            SendState(uid);
            return;
        }

        var existingIdx = game.Path.FindIndex(p => p.X == x && p.Y == y);
        if (existingIdx >= 0)
        {
            game.Path.RemoveRange(existingIdx + 1, game.Path.Count - existingIdx - 1);
            SendState(uid);
            return;
        }

        if (game.Path.Count == 0)
        {
            if (x == game.StartX && y == game.StartY)
                game.Path.Add((x, y));
            return;
        }

        var last = game.Path[^1];
        var dx = Math.Abs(x - last.X);
        var dy = Math.Abs(y - last.Y);

        if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
        {
            game.Path.Add((x, y));
            game.PathComplete = x == game.EndX && y == game.EndY;
        }

        SendState(uid);
    }

    private void OnResetPath(EntityUid uid, FrostheimNavConsoleComponent comp, NavConsoleResetPathMessage args)
    {
        if (!_games.TryGetValue(uid, out var game) || game.Phase != NavGamePhase.Pathfinding)
            return;

        game.Path.Clear();
        game.Path.Add((game.StartX, game.StartY));
        game.PathComplete = false;
        SendState(uid);
    }

    private void OnCalibrate(EntityUid uid, FrostheimNavConsoleComponent comp, NavConsoleCalibrateMessage args)
    {
        if (!_games.TryGetValue(uid, out var game) || game.Phase != NavGamePhase.Calibration)
            return;

        if (args.SliderIndex < 0 || args.SliderIndex >= CalibrationSliders)
            return;

        game.CurrentValues[args.SliderIndex] = Math.Clamp(args.Value, 0f, 1f);
        SendState(uid);
    }

    private void OnConfirm(EntityUid uid, FrostheimNavConsoleComponent comp, NavConsoleConfirmMessage args)
    {
        if (!_games.TryGetValue(uid, out var game))
            return;

        if (game.Phase == NavGamePhase.Pathfinding && game.PathComplete)
        {
            game.Phase = NavGamePhase.Calibration;
            SendState(uid);
            return;
        }

        if (game.Phase == NavGamePhase.Calibration)
        {
            var allCalibrated = true;
            for (var i = 0; i < CalibrationSliders; i++)
            {
                if (Math.Abs(game.CurrentValues[i] - game.TargetValues[i]) > CalibrationTolerance)
                {
                    allCalibrated = false;
                    break;
                }
            }

            if (!allCalibrated)
                return;

            game.Phase = NavGamePhase.Complete;
            comp.NavigationComplete = true;
            Dirty(uid, comp);
            _games.Remove(uid);

            var gridUid = Transform(uid).GridUid;
            if (gridUid != null && TryComp<FrostheimShuttleComponent>(gridUid, out var shuttle))
            {
                shuttle.NavigationComplete = true;
                _shuttleSystem.UpdateReady(gridUid.Value, shuttle);
            }

            SendState(uid);
            _ui.CloseUi(uid, FrostheimNavConsoleUiKey.Key);
        }
    }

    private void SendState(EntityUid uid)
    {
        _games.TryGetValue(uid, out var game);

        var state = new FrostheimNavConsoleState
        {
            Phase = game?.Phase ?? NavGamePhase.Complete,
            Grid = game?.Grid,
            GridWidth = GridWidth,
            GridHeight = GridHeight,
            StartX = game?.StartX ?? 0,
            StartY = game?.StartY ?? 0,
            EndX = game?.EndX ?? 0,
            EndY = game?.EndY ?? 0,
            CurrentPath = game?.Path ?? new List<(int, int)>(),
            PathComplete = game?.PathComplete ?? false,
            TargetValues = game?.TargetValues ?? Array.Empty<float>(),
            CurrentValues = game?.CurrentValues ?? Array.Empty<float>(),
            Tolerance = CalibrationTolerance,
        };

        _ui.SetUiState(uid, FrostheimNavConsoleUiKey.Key, state);
    }

    private NavGameData GenerateGame()
    {
        var grid = new NavGridCell[GridWidth * GridHeight];
        int startX, startY, endX, endY;

        while (true)
        {
            for (var i = 0; i < grid.Length; i++)
                grid[i] = _random.NextFloat() < AsteroidChance ? NavGridCell.Asteroid : NavGridCell.Empty;

            startX = 0;
            startY = _random.Next(GridHeight);
            endX = GridWidth - 1;
            endY = _random.Next(GridHeight);

            grid[startY * GridWidth + startX] = NavGridCell.Empty;
            grid[endY * GridWidth + endX] = NavGridCell.Empty;

            if (HasPath(grid, startX, startY, endX, endY))
                break;
        }

        var targetValues = new float[CalibrationSliders];
        var currentValues = new float[CalibrationSliders];
        for (var i = 0; i < CalibrationSliders; i++)
        {
            targetValues[i] = 0.15f + _random.NextFloat() * 0.7f;
            currentValues[i] = _random.NextFloat();
        }

        var game = new NavGameData
        {
            Phase = NavGamePhase.Pathfinding,
            Grid = grid,
            StartX = startX,
            StartY = startY,
            EndX = endX,
            EndY = endY,
            TargetValues = targetValues,
            CurrentValues = currentValues,
        };

        game.Path.Add((startX, startY));
        return game;
    }

    private static bool HasPath(NavGridCell[] grid, int startX, int startY, int endX, int endY)
    {
        var visited = new bool[GridWidth * GridHeight];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((startX, startY));
        visited[startY * GridWidth + startX] = true;

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            if (cx == endX && cy == endY)
                return true;

            for (var d = 0; d < 4; d++)
            {
                var nx = cx + dx[d];
                var ny = cy + dy[d];

                if (nx < 0 || nx >= GridWidth || ny < 0 || ny >= GridHeight)
                    continue;

                var idx = ny * GridWidth + nx;
                if (visited[idx] || grid[idx] == NavGridCell.Asteroid)
                    continue;

                visited[idx] = true;
                queue.Enqueue((nx, ny));
            }
        }

        return false;
    }

    private sealed class NavGameData
    {
        public NavGamePhase Phase;
        public NavGridCell[] Grid = Array.Empty<NavGridCell>();
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;
        public List<(int X, int Y)> Path = new();
        public bool PathComplete;
        public float[] TargetValues = Array.Empty<float>();
        public float[] CurrentValues = Array.Empty<float>();
    }
}
