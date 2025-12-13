// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldBuyPremiumCard : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Font _titleFont = default!;
    private Font _messageFont = default!;

    private readonly Color _bgColor = Color.FromHex("#0f0a1e");
    private readonly Color _borderColor = Color.FromHex("#6d5a8a");
    private readonly Color _titleColor = Color.FromHex("#d4a574");
    private readonly Color _messageColor = Color.FromHex("#8d7aaa");

    private const int TitleFontSize = 14;
    private const int MessageFontSize = 11;

    private EmeraldButton _buyButton = default!;

    public event Action? OnBuyPressed;

    public EmeraldBuyPremiumCard()
    {
        IoCManager.InjectDependencies(this);
        UpdateFonts();
        BuildUI();
    }

    private void UpdateFonts()
    {
        _titleFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(TitleFontSize * UIScale));
        _messageFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(MessageFontSize * UIScale));
    }

    private void BuildUI()
    {
        _buyButton = new EmeraldButton
        {
            Text = "КУПИТЬ ПРЕМИУМ",
            MinSize = new Vector2(160, 0)
        };
        _buyButton.OnPressed += () => OnBuyPressed?.Invoke();
        AddChild(_buyButton);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var width = float.IsPositiveInfinity(availableSize.X) ? 400 : availableSize.X;

        if (_buyButton != null)
        {
            _buyButton.Measure(availableSize);
        }

        return new Vector2(width, 130);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        if (_buyButton != null)
        {
            var buttonSize = _buyButton.DesiredSize;
            var buttonX = (finalSize.X - buttonSize.X) / 2f;
            var buttonY = finalSize.Y - buttonSize.Y - 14f;
            _buyButton.Arrange(new UIBox2(buttonX, buttonY, buttonX + buttonSize.X, buttonY + buttonSize.Y));
        }

        return finalSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.6f));
        DrawBorder(handle, rect, _borderColor);

        var padding = 14f;
        var currentY = padding;

        var line1Text = "У ВАС НЕТ ПРЕМИУМА";
        var line1Width = GetTextWidth(line1Text, _titleFont);
        var line1X = (PixelSize.X - line1Width) / 2f;

        handle.DrawString(_titleFont, new Vector2(line1X, currentY), line1Text, 1f, _titleColor);
        currentY += _titleFont.GetLineHeight(1f) + 4f;

        var line2Text = "У ВАС НЕТУ БОНУСОВ =(";
        var line2Width = GetTextWidth(line2Text, _messageFont);
        var line2X = (PixelSize.X - line2Width) / 2f;

        handle.DrawString(_messageFont, new Vector2(line2X, currentY), line2Text, 1f, _messageColor);
        currentY += _messageFont.GetLineHeight(1f) + 4f;

        var line3Text = "ИСПРАВЬТЕ ЭТО!";
        var line3Width = GetTextWidth(line3Text, _messageFont);
        var line3X = (PixelSize.X - line3Width) / 2f;

        handle.DrawString(_messageFont, new Vector2(line3X, currentY), line3Text, 1f, _messageColor);
    }

    private void DrawBorder(DrawingHandleScreen handle, UIBox2 rect, Color color)
    {
        handle.DrawRect(new UIBox2(rect.Left, rect.Top, rect.Right, rect.Top + 1), color);
        handle.DrawRect(new UIBox2(rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom), color);
        handle.DrawRect(new UIBox2(rect.Left, rect.Top, rect.Left + 1, rect.Bottom), color);
        handle.DrawRect(new UIBox2(rect.Right - 1, rect.Top, rect.Right, rect.Bottom), color);
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

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFonts();
        InvalidateMeasure();
    }
}
