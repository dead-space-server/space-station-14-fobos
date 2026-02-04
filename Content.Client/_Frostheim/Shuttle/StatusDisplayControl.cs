using System.Numerics;
using Content.Shared._Frostheim.Shuttle;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Frostheim.Shuttle;

public sealed class StatusDisplayControl : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private static readonly Color BgColor = Color.FromHex("#0a0e14");
    private static readonly Color SectionBg = Color.FromHex("#0f1520");
    private static readonly Color SectionBorder = Color.FromHex("#1a2a3a");
    private static readonly Color LabelColor = Color.FromHex("#5a7a9a");
    private static readonly Color ValueColor = Color.FromHex("#7a9ab0");
    private static readonly Color GoodColor = Color.FromHex("#2a8a2a");
    private static readonly Color BadColor = Color.FromHex("#8b2020");
    private static readonly Color WarningColor = Color.FromHex("#c4a030");
    private static readonly Color BarBg = Color.FromHex("#1a1a2a");
    private static readonly Color BarFill = Color.FromHex("#2a6a4a");
    private static readonly Color BarFillFull = Color.FromHex("#2a8a2a");
    private static readonly Color ReadyBg = Color.FromHex("#1a2a1a");
    private static readonly Color ReadyBorder = Color.FromHex("#2a8a2a");
    private static readonly Color NotReadyBg = Color.FromHex("#2a1a1a");
    private static readonly Color NotReadyBorder = Color.FromHex("#8b2020");

    private Font _font;
    private Font _titleFont;
    private Font _bigFont;

    private int _thrustersNeeded;
    private int _thrustersCurrent;
    private bool _gyroscopeRequired;
    private bool _gyroscopeInstalled;
    private bool _navigationComplete;
    private bool _ready;

    private const float SectionH = 60f;
    private const float SectionGap = 8f;
    private const float SectionPad = 10f;

    public StatusDisplayControl()
    {
        IoCManager.InjectDependencies(this);
        HorizontalExpand = true;
        VerticalExpand = true;

        var fontRes = _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf");
        _font = new VectorFont(fontRes, 10);
        _titleFont = new VectorFont(fontRes, 9);
        _bigFont = new VectorFont(fontRes, 14);
    }

    public void UpdateState(FrostheimStatusConsoleState state)
    {
        _thrustersNeeded = state.ThrustersNeeded;
        _thrustersCurrent = state.ThrustersCurrent;
        _gyroscopeRequired = state.GyroscopeRequired;
        _gyroscopeInstalled = state.GyroscopeInstalled;
        _navigationComplete = state.NavigationComplete;
        _ready = state.Ready;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        handle.DrawRect(PixelSizeBox, BgColor);

        var y = 8f * UIScale;
        var margin = 8f * UIScale;
        var sectionW = PixelWidth - margin * 2;

        y = DrawThrusterSection(handle, margin, y, sectionW);
        y += SectionGap * UIScale;
        y = DrawGyroscopeSection(handle, margin, y, sectionW);
        y += SectionGap * UIScale;
        y = DrawNavigationSection(handle, margin, y, sectionW);
        y += SectionGap * UIScale;
        DrawReadySection(handle, margin, y, sectionW);
    }

    private float DrawThrusterSection(DrawingHandleScreen handle, float x, float y, float w)
    {
        var h = SectionH * UIScale;
        DrawSectionBox(handle, x, y, w, h);

        var pad = SectionPad * UIScale;
        handle.DrawString(_titleFont, new Vector2(x + pad, y + 4f * UIScale), "ДВИГАТЕЛИ", UIScale, LabelColor);

        var thrusterOk = _thrustersCurrent >= _thrustersNeeded;
        var statusText = $"{_thrustersCurrent} / {_thrustersNeeded}";
        var statusColor = thrusterOk ? GoodColor : (_thrustersCurrent > 0 ? WarningColor : BadColor);
        handle.DrawString(_font, new Vector2(x + w - pad - GetTextWidth(statusText, _font), y + 4f * UIScale), statusText, UIScale, statusColor);

        var barY = y + 28f * UIScale;
        var barH = 16f * UIScale;
        var barX = x + pad;
        var barW = w - pad * 2;

        handle.DrawRect(new UIBox2(barX, barY, barX + barW, barY + barH), BarBg);

        if (_thrustersNeeded > 0)
        {
            var fill = Math.Clamp((float)_thrustersCurrent / _thrustersNeeded, 0f, 1f);
            var fillColor = fill >= 1f ? BarFillFull : BarFill;
            handle.DrawRect(new UIBox2(barX, barY, barX + barW * fill, barY + barH), fillColor);
        }

        var tagText = thrusterOk ? "OK" : "НЕДОСТАТОЧНО";
        var tagColor = thrusterOk ? GoodColor : BadColor;
        var tagX = barX + barW / 2f - GetTextWidth(tagText, _titleFont) / 2f;
        handle.DrawString(_titleFont, new Vector2(tagX, barY + 1f * UIScale), tagText, UIScale, tagColor);

        return y + h;
    }

    private float DrawGyroscopeSection(DrawingHandleScreen handle, float x, float y, float w)
    {
        var h = 40f * UIScale;
        DrawSectionBox(handle, x, y, w, h);

        var pad = SectionPad * UIScale;
        handle.DrawString(_titleFont, new Vector2(x + pad, y + 10f * UIScale), "ГИРОСКОП", UIScale, LabelColor);

        string statusText;
        Color statusColor;

        if (!_gyroscopeRequired)
        {
            statusText = "НЕ ТРЕБУЕТСЯ";
            statusColor = ValueColor;
        }
        else if (_gyroscopeInstalled)
        {
            statusText = "УСТАНОВЛЕН";
            statusColor = GoodColor;
        }
        else
        {
            statusText = "ОТСУТСТВУЕТ";
            statusColor = BadColor;
        }

        handle.DrawString(_font, new Vector2(x + w - pad - GetTextWidth(statusText, _font), y + 10f * UIScale), statusText, UIScale, statusColor);

        return y + h;
    }

    private float DrawNavigationSection(DrawingHandleScreen handle, float x, float y, float w)
    {
        var h = 40f * UIScale;
        DrawSectionBox(handle, x, y, w, h);

        var pad = SectionPad * UIScale;
        handle.DrawString(_titleFont, new Vector2(x + pad, y + 10f * UIScale), "НАВИГАЦИЯ", UIScale, LabelColor);

        var statusText = _navigationComplete ? "КУРС ПРОЛОЖЕН" : "НЕ НАСТРОЕНА";
        var statusColor = _navigationComplete ? GoodColor : BadColor;
        handle.DrawString(_font, new Vector2(x + w - pad - GetTextWidth(statusText, _font), y + 10f * UIScale), statusText, UIScale, statusColor);

        return y + h;
    }

    private void DrawReadySection(DrawingHandleScreen handle, float x, float y, float w)
    {
        var h = 44f * UIScale;
        var bg = _ready ? ReadyBg : NotReadyBg;
        var border = _ready ? ReadyBorder : NotReadyBorder;

        handle.DrawRect(new UIBox2(x, y, x + w, y + h), bg);
        handle.DrawLine(new Vector2(x, y), new Vector2(x + w, y), border);
        handle.DrawLine(new Vector2(x + w, y), new Vector2(x + w, y + h), border);
        handle.DrawLine(new Vector2(x + w, y + h), new Vector2(x, y + h), border);
        handle.DrawLine(new Vector2(x, y + h), new Vector2(x, y), border);

        var text = _ready ? ">>> СИСТЕМЫ ГОТОВЫ К ЗАПУСКУ <<<" : ">>> СИСТЕМЫ НЕ ГОТОВЫ <<<";
        var color = _ready ? GoodColor : BadColor;
        var textW = GetTextWidth(text, _bigFont);
        var textX = x + w / 2f - textW / 2f;
        handle.DrawString(_bigFont, new Vector2(textX, y + 10f * UIScale), text, UIScale, color);
    }

    private void DrawSectionBox(DrawingHandleScreen handle, float x, float y, float w, float h)
    {
        handle.DrawRect(new UIBox2(x, y, x + w, y + h), SectionBg);
        handle.DrawLine(new Vector2(x, y), new Vector2(x + w, y), SectionBorder);
        handle.DrawLine(new Vector2(x + w, y), new Vector2(x + w, y + h), SectionBorder);
        handle.DrawLine(new Vector2(x + w, y + h), new Vector2(x, y + h), SectionBorder);
        handle.DrawLine(new Vector2(x, y + h), new Vector2(x, y), SectionBorder);
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
