using System.Numerics;
using Content.Client.Items.Systems;
using Content.Client.TextScreen;
using Content.Shared.DeadSpace.SignBoard;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.TextScreen;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;

namespace Content.Client.DeadSpace.SignBoard;

public sealed class SignBoardVisualsSystem : EntitySystem
{
    [Dependency] private readonly ItemSystem _itemSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private double _lastEyeTheta;
    private bool _pendingRefresh;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SignBoardComponent, GetInhandVisualsEvent>(OnGetInhandVisuals);
        SubscribeLocalEvent<SignBoardComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<HandsComponent, MoveEvent>(OnPlayerMove);

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.CameraRotateLeft, InputCmdHandler.FromDelegate(_ => _pendingRefresh = true, handle: false, outsidePrediction: true))
            .Bind(EngineKeyFunctions.CameraRotateRight, InputCmdHandler.FromDelegate(_ => _pendingRefresh = true, handle: false, outsidePrediction: true))
            .Bind(EngineKeyFunctions.CameraReset, InputCmdHandler.FromDelegate(_ => _pendingRefresh = true, handle: false, outsidePrediction: true))
            .Register<SignBoardVisualsSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<SignBoardVisualsSystem>();
        base.Shutdown();
    }

    public override void FrameUpdate(float frameTime)
    {
        var currentTheta = _eye.CurrentEye.Rotation.Theta;
        var rotationChanged = Math.Abs(currentTheta - _lastEyeTheta) >= 0.001;
        _lastEyeTheta = currentTheta;

        if (!rotationChanged && !_pendingRefresh)
            return;

        _pendingRefresh = false;
        DoRefresh();
    }

    private void OnPlayerMove(EntityUid uid, HandsComponent hands, ref MoveEvent args)
    {
        if (args.NewRotation == args.OldRotation)
            return;

        foreach (var held in _hands.EnumerateHeld((uid, hands)))
        {
            if (HasComp<SignBoardComponent>(held))
                _itemSystem.VisualsChanged(held);
        }
    }

    private void DoRefresh()
    {
        var player = _playerManager.LocalEntity;
        if (player == null || !TryComp<HandsComponent>(player, out var hands))
            return;

        foreach (var held in _hands.EnumerateHeld((player.Value, hands)))
        {
            if (HasComp<SignBoardComponent>(held))
                _itemSystem.VisualsChanged(held);
        }
    }

    private void OnAppearanceChanged(EntityUid uid, SignBoardComponent component, ref AppearanceChangeEvent args)
    {
        _itemSystem.VisualsChanged(uid);
    }

    private void OnGetInhandVisuals(EntityUid uid, SignBoardComponent sign, GetInhandVisualsEvent args)
    {
        if (!TryComp<TextScreenVisualsComponent>(uid, out var screen))
            return;

        var text = sign.Text;

        if (string.IsNullOrEmpty(text))
            return;

        var rotation = Transform(args.User).WorldRotation + _eye.CurrentEye.Rotation;
        if (rotation.GetCardinalDir() != Direction.South)
            return;

        var pixelSize = TextScreenVisualsComponent.PixelSize;
        var charWidth = 4;
        var rows = screen.Rows;
        var rowLength = screen.RowLength;
        var rowOffset = screen.RowOffset;

        for (var rowIdx = 0; rowIdx < Math.Min(rows, (text.Length - 1) / rowLength + 1); rowIdx++)
        {
            var start = rowIdx * rowLength;
            var len = Math.Min(text.Length - start, rowLength);
            var row = text.Substring(start, len).Trim();
            if (string.IsNullOrEmpty(row))
                continue;

            var min = Math.Min(row.Length, rowLength);
            for (var chr = 0; chr < min; chr++)
            {
                var state = TextScreenSystem.GetStateFromChar(row[chr]);
                if (state == null)
                    continue;

                var charLayer = new PrototypeLayerData
                {
                    RsiPath = "Effects/text.rsi",
                    State = state,
                    Color = screen.Color,
                    Offset = Vector2.Multiply(
                        new Vector2((chr - min / 2f + 0.5f) * charWidth, -rowIdx * rowOffset),
                        pixelSize) + screen.TextOffset
                };

                var key = $"signboard-text-{args.Location.ToString().ToLowerInvariant()}-{rowIdx}-{chr}";
                args.Layers.Add((key, charLayer));
            }
        }
    }
}
