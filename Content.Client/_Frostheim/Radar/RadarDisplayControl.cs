using System.Numerics;
using Content.Shared._Frostheim.Radar;
using Content.Shared._Frostheim.Shuttle;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client._Frostheim.Radar;

public sealed class RadarDisplayControl : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private static readonly Color BgColor = Color.FromHex("#080c12");
    private static readonly Color RingColor = Color.FromHex("#1a2a3a");
    private static readonly Color CrosshairColor = Color.FromHex("#0f1a24");
    private static readonly Color TextColor = Color.FromHex("#7a9ab0");
    private static readonly Color TextDimColor = Color.FromHex("#3a5a6a");
    private static readonly Color CenterColor = Color.FromHex("#2a8a2a");
    private static readonly Color SweepColor = Color.FromHex("#1a4a2a");
    private static readonly Color SweepBright = Color.FromHex("#2a6a3a");
    private static readonly Color ThrusterColor = Color.FromHex("#c4a030");
    private static readonly Color GyroscopeColor = Color.FromHex("#4a9ac0");
    private static readonly Color RangeTextColor = Color.FromHex("#2a3a4a");
    private static readonly Color NoSignalColor = Color.FromHex("#8b2020");
    private static readonly Color GridDotColor = Color.FromHex("#0d1620");

    private Font _font;
    private Font _labelFont;
    private Font _bigFont;

    private List<RadarBlip> _blips = new();
    private int _totalThrusters;
    private int _totalGyroscopes;
    private float _maxRange = 200f;
    private float _sweepAngle;
    private float _time;

    private const int CircleSegments = 72;
    private const float RadarPadding = 36f;
    private const float LegendHeight = 52f;
    private const float SweepSpeed = 1.8f;

    public RadarDisplayControl()
    {
        IoCManager.InjectDependencies(this);
        HorizontalExpand = true;
        VerticalExpand = true;

        var fontRes = _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf");
        _font = new VectorFont(fontRes, 11);
        _labelFont = new VectorFont(fontRes, 12);
        _bigFont = new VectorFont(fontRes, 14);
    }

    public void UpdateState(FrostheimRadarUpdateMessage state)
    {
        _blips = state.Blips;
        _totalThrusters = state.TotalThrusters;
        _totalGyroscopes = state.TotalGyroscopes;
        _maxRange = state.MaxRange;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        _time += args.DeltaSeconds;
        // Sweep rotates clockwise
        _sweepAngle -= args.DeltaSeconds * SweepSpeed;
        if (_sweepAngle < 0)
            _sweepAngle += MathF.PI * 2f;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        handle.DrawRect(PixelSizeBox, BgColor);

        var legendArea = LegendHeight * UIScale;
        var radarAreaH = PixelHeight - legendArea;
        var radius = (MathF.Min(PixelWidth, radarAreaH) - RadarPadding * 2f * UIScale) / 2f;
        var center = new Vector2(PixelWidth / 2f, radarAreaH / 2f);

        // Background grid dots
        DrawGridDots(handle, center, radius);

        // Concentric distance rings
        DrawCircle(handle, center, radius, RingColor);
        DrawCircle(handle, center, radius * 0.75f, RingColor.WithAlpha(0.5f));
        DrawCircle(handle, center, radius * 0.5f, RingColor);
        DrawCircle(handle, center, radius * 0.25f, RingColor.WithAlpha(0.5f));

        // Crosshair lines
        handle.DrawLine(new Vector2(center.X, center.Y - radius), new Vector2(center.X, center.Y + radius), CrosshairColor);
        handle.DrawLine(new Vector2(center.X - radius, center.Y), new Vector2(center.X + radius, center.Y), CrosshairColor);

        // Diagonal crosshairs
        var diag = radius * 0.707f;
        handle.DrawLine(new Vector2(center.X - diag, center.Y - diag), new Vector2(center.X + diag, center.Y + diag), CrosshairColor.WithAlpha(0.3f));
        handle.DrawLine(new Vector2(center.X + diag, center.Y - diag), new Vector2(center.X - diag, center.Y + diag), CrosshairColor.WithAlpha(0.3f));

        // Cardinal direction labels
        var cardOffset = 14f * UIScale;
        handle.DrawString(_labelFont, new Vector2(center.X - 4f * UIScale, center.Y - radius - cardOffset), "N", UIScale, TextDimColor);
        handle.DrawString(_labelFont, new Vector2(center.X - 4f * UIScale, center.Y + radius + 4f * UIScale), "S", UIScale, TextDimColor);
        handle.DrawString(_labelFont, new Vector2(center.X - radius - cardOffset - 2f * UIScale, center.Y - 6f * UIScale), "W", UIScale, TextDimColor);
        handle.DrawString(_labelFont, new Vector2(center.X + radius + 6f * UIScale, center.Y - 6f * UIScale), "E", UIScale, TextDimColor);

        // Range labels
        var r50 = (int)(_maxRange * 0.5f);
        var rMax = (int)_maxRange;
        handle.DrawString(_font, new Vector2(center.X + 4f * UIScale, center.Y - radius * 0.5f + 2f * UIScale), $"{r50}m", UIScale, RangeTextColor);
        handle.DrawString(_font, new Vector2(center.X + 4f * UIScale, center.Y - radius + 2f * UIScale), $"{rMax}m", UIScale, RangeTextColor);

        // Sweep line with glow trail
        DrawSweep(handle, center, radius);

        // Center marker
        var dotSize = 3f * UIScale;
        handle.DrawRect(new UIBox2(center.X - dotSize, center.Y - dotSize, center.X + dotSize, center.Y + dotSize), CenterColor);
        var crossLen = dotSize * 2.5f;
        handle.DrawLine(new Vector2(center.X - crossLen, center.Y), new Vector2(center.X + crossLen, center.Y), CenterColor.WithAlpha(0.4f));
        handle.DrawLine(new Vector2(center.X, center.Y - crossLen), new Vector2(center.X, center.Y + crossLen), CenterColor.WithAlpha(0.4f));

        // Blips with sweep ghost effect
        if (_blips.Count > 0)
        {
            foreach (var blip in _blips)
            {
                DrawBlip(handle, center, radius, blip);
            }
        }
        else
        {
            var blink = 0.4f + 0.4f * MathF.Sin(_time * 2f);
            var noSigText = "НЕТ СИГНАЛА";
            var noSigW = GetTextWidth(noSigText, _bigFont);
            handle.DrawString(_bigFont, new Vector2(center.X - noSigW / 2f, center.Y + 20f * UIScale), noSigText, UIScale, NoSignalColor.WithAlpha(blink));
        }

        // Legend bar
        DrawLegend(handle, radarAreaH);
    }

    private void DrawBlip(DrawingHandleScreen handle, Vector2 center, float radius, RadarBlip blip)
    {
        var normalizedDist = MathF.Min(blip.Distance / _maxRange, 0.95f);
        var screenX = center.X + normalizedDist * radius * MathF.Cos(blip.Angle);
        var screenY = center.Y - normalizedDist * radius * MathF.Sin(blip.Angle);

        var baseColor = blip.Type == FrostheimPartType.Thruster ? ThrusterColor : GyroscopeColor;

        // Ghost effect: calculate how far the sweep has passed this blip
        // angDist = 0 means sweep just passed, angDist approaching 2π means about to be swept
        var angDist = (blip.Angle - _sweepAngle) % (MathF.PI * 2f);
        if (angDist < 0)
            angDist += MathF.PI * 2f;

        // Fast cubic falloff: bright when just swept, fades fast to match sweep speed
        var t = angDist / (MathF.PI * 2f);
        var alpha = MathF.Pow(1f - t, 3f);
        alpha = MathF.Max(0.04f, alpha);

        // Skip nearly invisible blips entirely
        if (alpha < 0.05f)
            return;

        var color = baseColor.WithAlpha(alpha);
        var pos = new Vector2(screenX, screenY);
        var blipSize = 5f * UIScale;

        // Draw blip diamond
        handle.DrawLine(new Vector2(pos.X, pos.Y - blipSize), new Vector2(pos.X + blipSize, pos.Y), color);
        handle.DrawLine(new Vector2(pos.X + blipSize, pos.Y), new Vector2(pos.X, pos.Y + blipSize), color);
        handle.DrawLine(new Vector2(pos.X, pos.Y + blipSize), new Vector2(pos.X - blipSize, pos.Y), color);
        handle.DrawLine(new Vector2(pos.X - blipSize, pos.Y), new Vector2(pos.X, pos.Y - blipSize), color);

        // Inner filled square
        var innerSize = 2f * UIScale;
        handle.DrawRect(new UIBox2(pos.X - innerSize, pos.Y - innerSize, pos.X + innerSize, pos.Y + innerSize), color);

        // Distance text — same alpha as blip, fades together
        var distText = $"{(int)blip.Distance}m";
        handle.DrawString(_font, new Vector2(pos.X + blipSize + 3f * UIScale, pos.Y - 6f * UIScale), distText, UIScale, color);
    }

    private void DrawSweep(DrawingHandleScreen handle, Vector2 center, float radius)
    {
        // Main sweep line (bright)
        var endX = center.X + radius * MathF.Cos(_sweepAngle);
        var endY = center.Y - radius * MathF.Sin(_sweepAngle);
        handle.DrawLine(center, new Vector2(endX, endY), SweepBright);

        // Glow trail behind the sweep (8 trailing lines)
        for (var i = 1; i <= 8; i++)
        {
            // Trail follows behind sweep (positive offset since sweep decreases)
            var trailAngle = _sweepAngle + i * 0.06f;
            var trailEndX = center.X + radius * MathF.Cos(trailAngle);
            var trailEndY = center.Y - radius * MathF.Sin(trailAngle);
            var alpha = 0.35f * (1f - i / 9f);
            handle.DrawLine(center, new Vector2(trailEndX, trailEndY), SweepColor.WithAlpha(alpha));
        }
    }

    private void DrawGridDots(DrawingHandleScreen handle, Vector2 center, float radius)
    {
        var dotR = 1f * UIScale;
        var step = radius / 5f;
        for (var ix = -5; ix <= 5; ix++)
        {
            for (var iy = -5; iy <= 5; iy++)
            {
                var dx = ix * step;
                var dy = iy * step;
                if (dx * dx + dy * dy > radius * radius)
                    continue;

                var pos = new Vector2(center.X + dx, center.Y + dy);
                handle.DrawRect(new UIBox2(pos.X - dotR, pos.Y - dotR, pos.X + dotR, pos.Y + dotR), GridDotColor);
            }
        }
    }

    private void DrawCircle(DrawingHandleScreen handle, Vector2 center, float radius, Color color)
    {
        var step = MathF.PI * 2f / CircleSegments;
        for (var i = 0; i < CircleSegments; i++)
        {
            var a1 = i * step;
            var a2 = (i + 1) * step;
            var p1 = new Vector2(center.X + radius * MathF.Cos(a1), center.Y + radius * MathF.Sin(a1));
            var p2 = new Vector2(center.X + radius * MathF.Cos(a2), center.Y + radius * MathF.Sin(a2));
            handle.DrawLine(p1, p2, color);
        }
    }

    private void DrawLegend(DrawingHandleScreen handle, float radarAreaH)
    {
        var y = radarAreaH + 6f * UIScale;
        var x = 12f * UIScale;
        var dotSize = 5f * UIScale;

        // Separator
        handle.DrawLine(new Vector2(4f * UIScale, radarAreaH), new Vector2(PixelWidth - 4f * UIScale, radarAreaH), RingColor);

        // Thruster legend
        handle.DrawRect(new UIBox2(x, y + 3f * UIScale, x + dotSize * 2, y + 3f * UIScale + dotSize * 2), ThrusterColor);
        handle.DrawString(_font, new Vector2(x + dotSize * 2 + 6f * UIScale, y), $"ДВГ: {_totalThrusters}", UIScale, ThrusterColor);

        // Gyroscope legend
        var gyroX = PixelWidth / 2f + 10f * UIScale;
        handle.DrawRect(new UIBox2(gyroX, y + 3f * UIScale, gyroX + dotSize * 2, y + 3f * UIScale + dotSize * 2), GyroscopeColor);
        handle.DrawString(_font, new Vector2(gyroX + dotSize * 2 + 6f * UIScale, y), $"ГИР: {_totalGyroscopes}", UIScale, GyroscopeColor);

        // Second row
        y += 22f * UIScale;
        handle.DrawString(_font, new Vector2(x, y), $"ДАЛЬН: {(int)_maxRange}m", UIScale, TextDimColor);

        var total = _totalThrusters + _totalGyroscopes;
        var totalText = $"ВСЕГО: {total}";
        var totalColor = total > 0 ? TextColor : NoSignalColor;
        handle.DrawString(_font, new Vector2(gyroX, y), totalText, UIScale, totalColor);
    }

    private float GetTextWidth(string text, Font font)
    {
        var width = 0f;
        foreach (var rune in text.EnumerateRunes())
        {
            var metrics = font.GetCharMetrics(rune, UIScale);
            if (metrics.HasValue)
                width += metrics.Value.Advance;
        }
        return width;
    }
}
