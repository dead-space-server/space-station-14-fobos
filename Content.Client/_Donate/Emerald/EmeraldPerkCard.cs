// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldPerkCard : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Font _titleFont = default!;
    private Font _valueFont = default!;

    private string _title = "";
    private string _value = "";
    private Color _valueColor = Color.White;

    private readonly Color _bgColor = Color.FromHex("#1a0f2e");
    private readonly Color _borderColor = Color.FromHex("#4a3a6a");
    private readonly Color _titleColor = Color.FromHex("#6d5a8a");

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            InvalidateMeasure();
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            InvalidateMeasure();
        }
    }

    public Color ValueColor
    {
        get => _valueColor;
        set
        {
            _valueColor = value;
            InvalidateMeasure();
        }
    }

    public EmeraldPerkCard()
    {
        IoCManager.InjectDependencies(this);
        UpdateFonts();
    }

    private void UpdateFonts()
    {
        _titleFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(9 * UIScale));
        _valueFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(11 * UIScale));
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return new Vector2(140, 60);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.8f));

        handle.DrawLine(rect.TopLeft, rect.TopRight, _borderColor);
        handle.DrawLine(rect.TopRight, rect.BottomRight, _borderColor);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, _borderColor);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, _borderColor);

        var maxTextWidth = PixelSize.X - 8f;
        var titleLines = WrapText(_title, maxTextWidth, _titleFont, 2);
        var titleLineHeight = _titleFont.GetLineHeight(1f);
        var valueLineHeight = _valueFont.GetLineHeight(1f);

        var totalContentHeight = titleLines.Count * titleLineHeight + 4f + valueLineHeight;
        var startY = (PixelSize.Y - totalContentHeight) / 2f;

        for (int i = 0; i < titleLines.Count; i++)
        {
            var line = titleLines[i];
            var lineWidth = GetTextWidth(line, _titleFont);
            var lineX = (PixelSize.X - lineWidth) / 2f;
            handle.DrawString(_titleFont, new Vector2(lineX, startY + i * titleLineHeight), line, 1f, _titleColor);
        }

        var valueWidth = GetTextWidth(_value, _valueFont);
        var valueX = (PixelSize.X - valueWidth) / 2f;
        var valueY = startY + titleLines.Count * titleLineHeight + 4f;
        handle.DrawString(_valueFont, new Vector2(valueX, valueY), _value, 1f, _valueColor);
    }

    private float GetTextWidth(string text, Font font)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        var width = 0f;
        foreach (var rune in text.EnumerateRunes())
        {
            var metrics = font.GetCharMetrics(rune, 1f);
            if (metrics.HasValue)
                width += metrics.Value.Advance;
        }
        return width;
    }

    private List<string> WrapText(string text, float maxWidth, Font font, int maxLines)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
            return lines;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var testWidth = GetTextWidth(testLine, font);

            if (testWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                if (lines.Count == maxLines - 1)
                {
                    var ellipsis = "..";
                    var availableWidth = maxWidth - GetTextWidth(ellipsis, font);
                    while (GetTextWidth(currentLine, font) > availableWidth && currentLine.Length > 0)
                    {
                        currentLine = currentLine.Substring(0, currentLine.Length - 1);
                    }
                    currentLine += ellipsis;
                    lines.Add(currentLine);
                    break;
                }

                lines.Add(currentLine);
                currentLine = word;

                if (lines.Count >= maxLines)
                {
                    var ellipsis = "..";
                    var availableWidth = maxWidth - GetTextWidth(ellipsis, font);
                    while (GetTextWidth(currentLine, font) > availableWidth && currentLine.Length > 0)
                    {
                        currentLine = currentLine.Substring(0, currentLine.Length - 1);
                    }
                    currentLine += ellipsis;
                    lines.Add(currentLine);
                    break;
                }
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine) && lines.Count < maxLines)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFonts();
        InvalidateMeasure();
    }
}
