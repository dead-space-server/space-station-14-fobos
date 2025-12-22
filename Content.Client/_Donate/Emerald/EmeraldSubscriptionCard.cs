// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldSubscriptionCard : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Font _nameFont = default!;
    private Font _infoFont = default!;

    private string _nameSub = "";
    private string _price = "";
    private string _dates = "";
    private int _itemCount;

    private readonly Color _bgColor = Color.FromHex("#1a0f2e");
    private readonly Color _borderColor = Color.FromHex("#4a3a6a");
    private readonly Color _nameColor = Color.FromHex("#c0b3da");
    private readonly Color _dateColor = Color.FromHex("#8975b5");
    private readonly Color _itemColor = Color.FromHex("#6d5a8a");

    private bool _isAdmin;
    private readonly Color _adminColor = Color.FromHex("#ff0000");
    private readonly Color _adminBorderColor = Color.FromHex("#d12e2e");

    public string NameSub
    {
        get => _nameSub;
        set
        {
            _nameSub = value;
            InvalidateMeasure();
        }
    }

    public string Price
    {
        get => _price;
        set
        {
            _price = value;
            InvalidateMeasure();
        }
    }

    public string Dates
    {
        get => _dates;
        set
        {
            _dates = value;
            InvalidateMeasure();
        }
    }

    public int ItemCount
    {
        get => _itemCount;
        set
        {
            _itemCount = value;
            InvalidateMeasure();
        }
    }

    public bool IsAdmin
    {
        get => _isAdmin;
        set
        {
            _isAdmin = value;
            InvalidateMeasure();
        }
    }

    public EmeraldSubscriptionCard()
    {
        IoCManager.InjectDependencies(this);
        UpdateFonts();
    }

    private void UpdateFonts()
    {
        _nameFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(11 * UIScale));
        _infoFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(9 * UIScale));
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var width = float.IsPositiveInfinity(availableSize.X) ? 300 : availableSize.X;
        return new Vector2(width, 70);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.8f));

        var borderColor = _isAdmin ? _adminBorderColor : _borderColor;
        handle.DrawLine(rect.TopLeft, rect.TopRight, borderColor);
        handle.DrawLine(rect.TopRight, rect.BottomRight, borderColor);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, borderColor);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, borderColor);

        var y = 8f;
        var x = 10f;

        var nameColor = _isAdmin ? _adminColor : _nameColor;
        handle.DrawString(_nameFont, new Vector2(x, y), _nameSub, 1f, nameColor);

        y += _nameFont.GetLineHeight(1f) + 4f;

        var infoText = $"{_price}  •  {_dates}";
        handle.DrawString(_infoFont, new Vector2(x, y), infoText, 1f, _dateColor);
        y += _infoFont.GetLineHeight(1f) + 4f;

        var itemText = $"{_itemCount} предметов подписки";
        handle.DrawString(_infoFont, new Vector2(x, y), itemText, 1f, _itemColor);
    }

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFonts();
        InvalidateMeasure();
    }
}
