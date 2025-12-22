// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldBatteryDisplay : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Font _font = default!;
    private Font _bigFont = default!;

    private int _amount;
    private Texture? _iconTexture;
    private string? _iconTexturePath;

    private readonly Color _batteryColor = Color.FromHex("#a589c9");
    private readonly Color _textColor = Color.FromHex("#c0b3da");
    private readonly Color _bgColor = Color.FromHex("#0f0a1e");
    private readonly Color _borderColor = Color.FromHex("#6d5a8a");

    private const float Padding = 10f;
    private const int BaseFontSize = 11;
    private const int BigFontSize = 13;

    public int Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            InvalidateMeasure();
        }
    }

    public string? IconTexturePath
    {
        get => _iconTexturePath;
        set
        {
            _iconTexturePath = value;
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    _iconTexture = _resourceCache.GetResource<TextureResource>(value).Texture;
                }
                catch
                {
                    _iconTexture = null;
                }
            }
            else
            {
                _iconTexture = null;
            }
            InvalidateMeasure();
        }
    }

    public EmeraldBatteryDisplay()
    {
        IoCManager.InjectDependencies(this);
        UpdateFont();
    }

    private void UpdateFont()
    {
        _font = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(BaseFontSize * UIScale));
        _bigFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(BigFontSize * UIScale));
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var width = float.IsPositiveInfinity(availableSize.X) ? 140 : Math.Max(100, availableSize.X);
        return new Vector2(width, 52);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.8f));
        DrawBorder(handle, rect, _borderColor);

        var iconSize = 32f;
        var iconX = Padding;
        var iconY = (PixelSize.Y - iconSize) / 2f;

        if (_iconTexture != null)
        {
            var iconRect = new UIBox2(iconX, iconY, iconX + iconSize, iconY + iconSize);
            handle.DrawTextureRect(_iconTexture, iconRect);
        }

        var labelText = "ЭНЕРГИЯ";
        var labelX = iconX + iconSize + 8f;
        var labelY = 8f;
        handle.DrawString(_font, new Vector2(labelX, labelY), labelText, 1f, _textColor);

        var amountText = _amount.ToString("N0");
        var amountX = labelX;
        var amountY = labelY + _font.GetLineHeight(1f) + 3;

        handle.DrawString(_bigFont, new Vector2(amountX, amountY), amountText, 1f, _batteryColor);
    }

    private void DrawBorder(DrawingHandleScreen handle, UIBox2 rect, Color color)
    {
        handle.DrawRect(new UIBox2(rect.Left, rect.Top, rect.Right, rect.Top + 1), color);
        handle.DrawRect(new UIBox2(rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom), color);
        handle.DrawRect(new UIBox2(rect.Left, rect.Top, rect.Left + 1, rect.Bottom), color);
        handle.DrawRect(new UIBox2(rect.Right - 1, rect.Top, rect.Right, rect.Bottom), color);
    }

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFont();
        InvalidateMeasure();
    }
}
