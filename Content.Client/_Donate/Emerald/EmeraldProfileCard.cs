// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldProfileCard : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Font _nameFont = default!;
    private Font _idFont = default!;

    private string _playerName = "";
    private string _playerId = "";

    private readonly Color _bgColor = Color.FromHex("#1a0f2e");
    private readonly Color _borderColor = Color.FromHex("#4a3a6a");
    private readonly Color _nameColor = Color.FromHex("#c0b3da");
    private readonly Color _idColor = Color.FromHex("#8975b5");

    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            InvalidateMeasure();
        }
    }

    public string PlayerId
    {
        get => _playerId;
        set
        {
            _playerId = value;
            InvalidateMeasure();
        }
    }

    public EmeraldProfileCard()
    {
        IoCManager.InjectDependencies(this);
        UpdateFonts();
    }

    private void UpdateFonts()
    {
        _nameFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(12 * UIScale));
        _idFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(9 * UIScale));
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var width = float.IsPositiveInfinity(availableSize.X) ? 300 : availableSize.X;
        return new Vector2(width, 50);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.8f));

        handle.DrawLine(rect.TopLeft, rect.TopRight, _borderColor);
        handle.DrawLine(rect.TopRight, rect.BottomRight, _borderColor);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, _borderColor);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, _borderColor);

        var nameY = 10f;
        handle.DrawString(_nameFont, new Vector2(10f, nameY), _playerName, 1f, _nameColor);

        var idY = nameY + _nameFont.GetLineHeight(1f) + 4f;
        handle.DrawString(_idFont, new Vector2(10f, idY), _playerId, 1f, _idColor);
    }

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFonts();
        InvalidateMeasure();
    }
}
