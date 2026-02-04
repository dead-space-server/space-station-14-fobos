using System.Numerics;
using Content.Shared._Frostheim.Shuttle;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client._Frostheim.Shuttle;

public sealed class NavCalibrationControl : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public event Action<int, float>? OnSliderChanged;

    private static readonly Color BgColor = Color.FromHex("#0a0e14");
    private static readonly Color TrackColor = Color.FromHex("#1a2a3a");
    private static readonly Color TargetZoneColor = Color.FromHex("#2a4a2a");
    private static readonly Color IndicatorFarColor = Color.FromHex("#8b2020");
    private static readonly Color IndicatorMidColor = Color.FromHex("#c4a030");
    private static readonly Color IndicatorCloseColor = Color.FromHex("#2a8a2a");
    private static readonly Color TextColor = Color.FromHex("#7a9ab0");
    private static readonly Color TextDimColor = Color.FromHex("#3a4a5a");
    private static readonly Color LabelColor = Color.FromHex("#5a7a9a");
    private static readonly Color ScaleMarkColor = Color.FromHex("#1a2a3a");

    private static readonly string[] SliderLabels = { "ДОЛГОТА", "ШИРОТА", "ВЫСОТА" };

    private float[] _targetValues = Array.Empty<float>();
    private float[] _currentValues = Array.Empty<float>();
    private float _tolerance;
    private int _draggingSlider = -1;
    private Font _font;
    private Font _labelFont;

    private const float SliderHeight = 80f;
    private const float SliderMarginY = 30f;
    private const float TrackMarginX = 80f;
    private const float TrackMarginRight = 60f;
    private const float IndicatorWidth = 4f;
    private const float TopPadding = 20f;

    public NavCalibrationControl()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;
        HorizontalExpand = true;
        VerticalExpand = true;

        var fontRes = _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf");
        _font = new VectorFont(fontRes, 9);
        _labelFont = new VectorFont(fontRes, 10);
    }

    public void UpdateState(FrostheimNavConsoleState state)
    {
        _targetValues = state.TargetValues;
        _currentValues = state.CurrentValues;
        _tolerance = state.Tolerance;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (_targetValues.Length == 0)
            return;

        handle.DrawRect(PixelSizeBox, BgColor);

        var headerText = ":: КООРДИНАТНАЯ КАЛИБРОВКА ::";
        var headerPos = new Vector2(PixelWidth * 0.5f - headerText.Length * 3.5f * UIScale, 4f * UIScale);
        handle.DrawString(_labelFont, headerPos, headerText, UIScale, TextColor);

        for (var i = 0; i < _targetValues.Length; i++)
        {
            DrawSlider(handle, i);
        }
    }

    private void DrawSlider(DrawingHandleScreen handle, int index)
    {
        var trackLeft = TrackMarginX * UIScale;
        var trackRight = PixelWidth - TrackMarginRight * UIScale;
        var trackWidth = trackRight - trackLeft;
        var yBase = (TopPadding + index * (SliderHeight + SliderMarginY)) * UIScale;

        var labelPos = new Vector2(8f * UIScale, yBase + 4f * UIScale);
        handle.DrawString(_labelFont, labelPos, SliderLabels[index], UIScale, LabelColor);

        var trackY = yBase + 30f * UIScale;
        var trackH = 20f * UIScale;
        var trackRect = new UIBox2(trackLeft, trackY, trackRight, trackY + trackH);
        handle.DrawRect(trackRect, TrackColor);

        for (var s = 0; s <= 10; s++)
        {
            var sx = trackLeft + trackWidth * s / 10f;
            var markH = s % 5 == 0 ? 8f * UIScale : 4f * UIScale;
            handle.DrawLine(
                new Vector2(sx, trackY + trackH),
                new Vector2(sx, trackY + trackH + markH),
                ScaleMarkColor);

            if (s % 5 == 0)
            {
                var valText = (s * 10).ToString();
                handle.DrawString(_font, new Vector2(sx - 6f * UIScale, trackY + trackH + markH + 2f * UIScale), valText, UIScale, TextDimColor);
            }
        }

        var target = _targetValues[index];
        var tolerancePixels = _tolerance * trackWidth;
        var targetX = trackLeft + target * trackWidth;
        var zoneRect = new UIBox2(
            targetX - tolerancePixels,
            trackY,
            targetX + tolerancePixels,
            trackY + trackH);
        handle.DrawRect(zoneRect, TargetZoneColor);

        handle.DrawLine(
            new Vector2(targetX, trackY - 4f * UIScale),
            new Vector2(targetX, trackY + trackH + 4f * UIScale),
            IndicatorCloseColor.WithAlpha(0.5f));

        var current = _currentValues[index];
        var currentX = trackLeft + current * trackWidth;
        var distance = MathF.Abs(current - target);
        Color indicatorColor;
        if (distance <= _tolerance)
            indicatorColor = IndicatorCloseColor;
        else if (distance <= _tolerance * 3f)
            indicatorColor = IndicatorMidColor;
        else
            indicatorColor = IndicatorFarColor;

        var indW = IndicatorWidth * UIScale;
        var indicatorRect = new UIBox2(
            currentX - indW / 2f,
            trackY - 6f * UIScale,
            currentX + indW / 2f,
            trackY + trackH + 6f * UIScale);
        handle.DrawRect(indicatorRect, indicatorColor);

        var triangle1 = currentX - 5f * UIScale;
        var triangle2 = currentX + 5f * UIScale;
        var triangleTop = trackY - 8f * UIScale;
        var triangleBot = trackY - 2f * UIScale;
        handle.DrawLine(new Vector2(triangle1, triangleTop), new Vector2(currentX, triangleBot), indicatorColor);
        handle.DrawLine(new Vector2(triangle2, triangleTop), new Vector2(currentX, triangleBot), indicatorColor);
        handle.DrawLine(new Vector2(triangle1, triangleTop), new Vector2(triangle2, triangleTop), indicatorColor);

        var statusText = distance <= _tolerance ? "SYNC" : $"{(int)(distance * 100)}%";
        var statusColor = distance <= _tolerance ? IndicatorCloseColor : indicatorColor;
        handle.DrawString(_font, new Vector2(trackRight + 4f * UIScale, trackY + 2f * UIScale), statusText, UIScale, statusColor);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        var slider = GetSliderAtPosition(args.RelativePosition);
        if (slider >= 0)
        {
            _draggingSlider = slider;
            UpdateSliderFromMouse(args.RelativePosition);
            args.Handle();
        }
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == EngineKeyFunctions.UIClick)
            _draggingSlider = -1;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (_draggingSlider >= 0)
            UpdateSliderFromMouse(args.RelativePosition);
    }

    private void UpdateSliderFromMouse(Vector2 relPos)
    {
        if (_draggingSlider < 0 || _draggingSlider >= _currentValues.Length)
            return;

        var trackLeft = TrackMarginX;
        var trackRight = PixelWidth / UIScale - TrackMarginRight;
        var trackWidth = trackRight - trackLeft;

        var value = (relPos.X - trackLeft) / trackWidth;
        value = Math.Clamp(value, 0f, 1f);

        OnSliderChanged?.Invoke(_draggingSlider, value);
    }

    private int GetSliderAtPosition(Vector2 relPos)
    {
        for (var i = 0; i < _targetValues.Length; i++)
        {
            var yBase = TopPadding + i * (SliderHeight + SliderMarginY);
            var trackY = yBase + 30f;
            var hitTop = trackY - 10f;
            var hitBottom = trackY + 40f;

            if (relPos.Y >= hitTop && relPos.Y <= hitBottom)
                return i;
        }

        return -1;
    }
}
