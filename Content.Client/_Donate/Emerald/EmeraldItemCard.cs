// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldItemCard : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    private Font _nameFont = default!;
    private Font _statusFont = default!;
    private Font _sourceFont = default!;

    private string _itemName = "";
    private string _protoId = "";
    private string? _timeFinish;
    private bool _timeAllways;
    private bool _isActive;
    private bool _isSpawned;
    private bool _isTimeUp;
    private string? _sourceSubscription;

    private readonly Color _bgColor = Color.FromHex("#1a0f2e");
    private readonly Color _borderColor = Color.FromHex("#4a3a6a");
    private readonly Color _nameColor = Color.FromHex("#c0b3da");
    private readonly Color _timeColor = Color.FromHex("#a589c9");
    private readonly Color _timeExpiringColor = Color.FromHex("#d4a574");
    private readonly Color _inactiveColor = Color.FromHex("#6d5a8a");
    private readonly Color _spriteBgColor = Color.FromHex("#0f0a1e");
    private readonly Color _hoverGlowColor = Color.FromHex("#6d5a8a");
    private readonly Color _spawnedColor = Color.FromHex("#4CAF50");
    private readonly Color _subscriptionColor = Color.FromHex("#d4a574");
    private readonly Color _purchasedColor = Color.FromHex("#8d7aaa");

    private SpriteView? _spriteView;
    private TextureRect? _textureRect;
    private PanelContainer? _spriteContainer;
    private Texture? _fallbackTexture;
    private bool _hovered;

    public event Action<string>? OnSpawnRequest;

    public string ItemName
    {
        get => _itemName;
        set
        {
            _itemName = value;
            InvalidateMeasure();
        }
    }

    public string ProtoId
    {
        get => _protoId;
        set
        {
            _protoId = value;
            UpdateSprite();
        }
    }

    public string? TimeFinish
    {
        get => _timeFinish;
        set
        {
            _timeFinish = value;
            InvalidateMeasure();
        }
    }

    public bool TimeAllways
    {
        get => _timeAllways;
        set
        {
            _timeAllways = value;
            InvalidateMeasure();
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            InvalidateMeasure();
        }
    }

    public bool IsSpawned
    {
        get => _isSpawned;
        set
        {
            _isSpawned = value;
            InvalidateMeasure();
        }
    }

    public bool IsTimeUp
    {
        get => _isTimeUp;
        set
        {
            _isTimeUp = value;
            InvalidateMeasure();
        }
    }

    public string? SourceSubscription
    {
        get => _sourceSubscription;
        set
        {
            _sourceSubscription = value;
            InvalidateMeasure();
        }
    }

    public bool IsFromSubscription => SourceSubscription != null;

    public EmeraldItemCard()
    {
        IoCManager.InjectDependencies(this);
        UpdateFonts();
        BuildSprite();
        MouseFilter = MouseFilterMode.Stop;
    }

    private void UpdateFonts()
    {
        _nameFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(10 * UIScale));
        _statusFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(9 * UIScale));
        _sourceFont = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(8 * UIScale));
    }

    private void BuildSprite()
    {
        _spriteContainer = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = _spriteBgColor.WithAlpha(0.4f)
            }
        };

        _spriteView = new SpriteView(_entMan)
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Stretch = SpriteView.StretchMode.Fit,
            Visible = false
        };

        _textureRect = new TextureRect
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Stretch = TextureRect.StretchMode.KeepCentered,
            Visible = false
        };

        _spriteContainer.AddChild(_spriteView);
        _spriteContainer.AddChild(_textureRect);
        AddChild(_spriteContainer);
    }

    private void UpdateSprite()
    {
        if (_spriteView == null || _textureRect == null)
            return;

        if (!string.IsNullOrEmpty(_protoId) && _protoManager.HasIndex<EntityPrototype>(_protoId))
        {
            try
            {
                var spawned = _entMan.SpawnEntity(_protoId, MapCoordinates.Nullspace);
                _spriteView.SetEntity(spawned);
                _spriteView.Visible = true;
                _spriteView.Scale = new Vector2(2, 2);
                _textureRect.Visible = false;
                return;
            }
            catch
            {
            }
        }

        _spriteView.Visible = false;
        _textureRect.Visible = true;

        if (_fallbackTexture == null)
        {
            try
            {
                _fallbackTexture = _resourceCache.GetResource<TextureResource>("/Textures/Interface/fallback.png").Texture;
            }
            catch
            {
                _fallbackTexture = null;
            }
        }

        _textureRect.Texture = _fallbackTexture;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return new Vector2(145, 200);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        if (_spriteContainer != null)
        {
            var spriteBox = new UIBox2(2, 2, finalSize.X - 2, 95);
            _spriteContainer.Arrange(spriteBox);
        }

        return finalSize;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        var bgAlpha = _isActive && !_isSpawned ? 0.8f : 0.5f;
        handle.DrawRect(rect, _bgColor.WithAlpha(bgAlpha));

        if (_hovered && _isActive && !_isSpawned && !string.IsNullOrEmpty(_protoId))
        {
            var glowRect = new UIBox2(rect.Left - 1, rect.Top - 1, rect.Right + 1, rect.Bottom + 1);
            handle.DrawRect(glowRect, _hoverGlowColor.WithAlpha(0.3f));
        }

        var borderColor = _isActive ? (IsFromSubscription ? _subscriptionColor : _borderColor) :
                        _isSpawned ? _spawnedColor :
                        _inactiveColor;
        handle.DrawLine(rect.TopLeft, rect.TopRight, borderColor);
        handle.DrawLine(rect.TopRight, rect.BottomRight, borderColor);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, borderColor);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, borderColor);

        var maxTextWidth = PixelSize.X - 8f;
        var lines = WrapText(_itemName, maxTextWidth, _nameFont, 3);
        var nameY = 99f;
        var lineHeight = _nameFont.GetLineHeight(1f);

        var nameColor = _isActive ? (IsFromSubscription ? _subscriptionColor : _borderColor) :
                        _isSpawned ? _spawnedColor :
                        _inactiveColor;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var lineWidth = GetTextWidth(line, _nameFont);
            var lineX = (PixelSize.X - lineWidth) / 2f;
            handle.DrawString(_nameFont, new Vector2(lineX, nameY + i * lineHeight), line, 1f, nameColor);
        }

        var statusY = nameY + lines.Count * lineHeight + 4f;

        if (IsFromSubscription)
        {
            var sourceText = _sourceSubscription!.ToUpper();
            var sourceWidth = GetTextWidth(sourceText, _sourceFont);
            var sourceX = (PixelSize.X - sourceWidth) / 2f;
            handle.DrawString(_sourceFont, new Vector2(sourceX, statusY), sourceText, 1f, _subscriptionColor);
            statusY += _sourceFont.GetLineHeight(1f) + 2f;
        }
        else
        {
            var purchasedText = "КУПЛЕНО";
            var purchasedWidth = GetTextWidth(purchasedText, _sourceFont);
            var purchasedX = (PixelSize.X - purchasedWidth) / 2f;
            handle.DrawString(_sourceFont, new Vector2(purchasedX, statusY), purchasedText, 1f, _purchasedColor);
            statusY += _sourceFont.GetLineHeight(1f) + 2f;
        }

        if (!_isActive)
        {
            var inactiveText = "НЕАКТИВЕН";
            var inactiveWidth = GetTextWidth(inactiveText, _statusFont);
            var inactiveX = (PixelSize.X - inactiveWidth) / 2f;
            handle.DrawString(_statusFont, new Vector2(inactiveX, statusY), inactiveText, 1f, _inactiveColor);
            return;
        }

        if (_isSpawned)
        {
            var spawnedText = "ЗАСПАВНЕНО";
            var spawnedWidth = GetTextWidth(spawnedText, _statusFont);
            var spawnedX = (PixelSize.X - spawnedWidth) / 2f;
            handle.DrawString(_statusFont, new Vector2(spawnedX, statusY), spawnedText, 1f, _spawnedColor);
            return;
        }

        if (_isTimeUp)
        {
            var timeUpText = "ВРЕМЯ ИСТЕКЛО";
            var timeUpWidth = GetTextWidth(timeUpText, _statusFont);
            var timeUpX = (PixelSize.X - timeUpWidth) / 2f;
            handle.DrawString(_statusFont, new Vector2(timeUpX, statusY), timeUpText, 1f, _timeExpiringColor);
            return;
        }

        string timeText;
        Color timeTextColor;

        if (_timeAllways)
        {
            timeText = "НАВСЕГДА";
            timeTextColor = _timeColor;
        }
        else if (!string.IsNullOrEmpty(_timeFinish))
        {
            if (DateTime.TryParse(_timeFinish, out var finishDate))
            {
                var now = DateTime.UtcNow;
                var timeLeft = finishDate - now;

                if (timeLeft.TotalDays < 7)
                {
                    timeTextColor = _timeExpiringColor;
                }
                else
                {
                    timeTextColor = _timeColor;
                }

                timeText = "до " + finishDate.ToString("dd.MM.yyyy");
            }
            else
            {
                timeText = "до " + _timeFinish;
                timeTextColor = _timeColor;
            }
        }
        else
        {
            return;
        }

        var timeWidth = GetTextWidth(timeText, _statusFont);
        var timeX = (PixelSize.X - timeWidth) / 2f;
        handle.DrawString(_statusFont, new Vector2(timeX, statusY), timeText, 1f, timeTextColor);
    }

    protected override void MouseEntered()
    {
        base.MouseEntered();
        if (!string.IsNullOrEmpty(_protoId) && _protoManager.HasIndex<EntityPrototype>(_protoId))
        {
            _hovered = true;
        }
        InvalidateMeasure();
    }

    protected override void MouseExited()
    {
        base.MouseExited();
        _hovered = false;
        InvalidateMeasure();
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (_isActive && !_isSpawned && !string.IsNullOrEmpty(_protoId) && _protoManager.HasIndex<EntityPrototype>(_protoId))
        {
            OnSpawnRequest?.Invoke(_protoId);
            args.Handle();
        }
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
