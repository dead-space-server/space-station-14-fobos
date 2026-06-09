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

namespace Content.Client.DeadSpace.SignBoard;

public sealed class SignBoardVisualsSystem : EntitySystem
{
    [Dependency] private readonly ItemSystem _itemSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private double _lastCombinedTheta;

    public override void Initialize()
    {
        SubscribeLocalEvent<SignBoardComponent, GetInhandVisualsEvent>(OnGetInhandVisuals);
        SubscribeLocalEvent<SignBoardComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    public override void FrameUpdate(float frameTime)
    {
        var player = _playerManager.LocalEntity;
        if (player == null)
            return;

        if (!TryComp<HandsComponent>(player, out var hands))
            return;

        var combined = Transform(player.Value).WorldRotation + _eye.CurrentEye.Rotation;
        if (Math.Abs(combined.Theta - _lastCombinedTheta) < 0.001)
            return;

        _lastCombinedTheta = combined.Theta;

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

        var rowCount = Math.Min(rows, (text.Length - 1) / rowLength + 1);
        for (var rowIdx = 0; rowIdx < rowCount; rowIdx++)
        {
            var start = rowIdx * rowLength;
            var len = Math.Min(text.Length - start, rowLength);
            var row = text.Substring(start, len).Trim();
            if (string.IsNullOrEmpty(row))
                continue;

            for (var chr = 0; chr < row.Length; chr++)
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
                        new Vector2((chr - row.Length / 2f + 0.5f) * charWidth, -rowIdx * rowOffset),
                        pixelSize) + screen.TextOffset
                };

                var key = $"signboard-text-{args.Location.ToString().ToLowerInvariant()}-{rowIdx}-{chr}";
                args.Layers.Add((key, charLayer));
            }
        }
    }
}
