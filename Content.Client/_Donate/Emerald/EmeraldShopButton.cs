// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldShopButton : Control
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int BaseFontSize = 12;

    private Font _font = default!;
    private string _text = "";
    private bool _hovered;
    private bool _pressed;

    private readonly Color _bgColor = Color.FromHex("#2a1a4a");
    private readonly Color _borderColor = Color.FromHex("#8d5aff");
    private readonly Color _textColor = Color.FromHex("#ffffff");
    private readonly Color _hoverGlowColor = Color.FromHex("#b388ff");
    private readonly Color _accentColor = Color.FromHex("#00FFAA");

    private float _glowIntensity = 0f;
    private float _pulsePhase = 0f;

    public event Action? OnPressed;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            InvalidateMeasure();
        }
    }

    public EmeraldShopButton()
    {
        IoCManager.InjectDependencies(this);
        MouseFilter = MouseFilterMode.Stop;
        UpdateFont();
    }

    private void UpdateFont()
    {
        _font = new VectorFont(
            _resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"),
            (int)(BaseFontSize * UIScale));
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var textWidth = GetTextWidth(_text.ToUpper());
        var paddingX = 20f;
        var paddingY = 12f;
        return new Vector2(textWidth + paddingX * 2, _font.GetLineHeight(1f) + paddingY);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _pulsePhase += (float)args.DeltaSeconds * 2f;
        if (_pulsePhase > MathF.PI * 2)
            _pulsePhase -= MathF.PI * 2;

        if (_hovered)
        {
            _glowIntensity = Math.Min(1f, _glowIntensity + (float)args.DeltaSeconds * 4f);
        }
        else
        {
            _glowIntensity = Math.Max(0f, _glowIntensity - (float)args.DeltaSeconds * 4f);
        }

        InvalidateMeasure();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.9f));

        var pulse = (MathF.Sin(_pulsePhase) + 1f) / 2f * 0.3f + 0.7f;
        var glowColor = _borderColor.WithAlpha(pulse * 0.8f);

        for (int i = 0; i < 3; i++)
        {
            var offset = i * 1f;
            var glowRect = new UIBox2(
                rect.Left - offset,
                rect.Top - offset,
                rect.Right + offset,
                rect.Bottom + offset
            );
            var alpha = (1f - i / 3f) * 0.4f;
            DrawBorder(handle, glowRect, glowColor.WithAlpha(alpha));
        }

        DrawBorder(handle, rect, _borderColor);

        if (_hovered)
        {
            var hoverGlowRect = new UIBox2(rect.Left - 2, rect.Top - 2, rect.Right + 2, rect.Bottom + 2);
            DrawBorder(handle, hoverGlowRect, _hoverGlowColor.WithAlpha(_glowIntensity * 0.6f));

            var innerGlowAlpha = _glowIntensity * 0.15f;
            handle.DrawRect(rect, _hoverGlowColor.WithAlpha(innerGlowAlpha));
        }

        var accentLineHeight = 2f;
        var accentRect = new UIBox2(rect.Left + 4, rect.Bottom - accentLineHeight - 4, rect.Right - 4, rect.Bottom - 4);
        handle.DrawRect(accentRect, _accentColor.WithAlpha(pulse));

        var displayText = _text.ToUpper();
        var textWidth = GetTextWidth(displayText);
        var textX = (PixelSize.X - textWidth) / 2f;
        var textY = (PixelSize.Y - _font.GetLineHeight(1f)) / 2f;

        if (_pressed)
        {
            textY += 1f;
        }

        var shadowOffset = new Vector2(1f, 1f);
        handle.DrawString(_font, new Vector2(textX, textY) + shadowOffset, displayText, 1f, Color.Black.WithAlpha(0.5f));
        handle.DrawString(_font, new Vector2(textX, textY), displayText, 1f, _textColor);
    }

    private void DrawBorder(DrawingHandleScreen handle, UIBox2 rect, Color color)
    {
        handle.DrawLine(rect.TopLeft, rect.TopRight, color);
        handle.DrawLine(rect.TopRight, rect.BottomRight, color);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, color);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, color);
    }

    private float GetTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        var width = 0f;
        foreach (var rune in text.EnumerateRunes())
        {
            var metrics = _font.GetCharMetrics(rune, 1f);
            if (metrics.HasValue)
                width += metrics.Value.Advance;
        }
        return width;
    }

    protected override void MouseEntered()
    {
        base.MouseEntered();
        _hovered = true;
        UserInterfaceManager.HoverSound();
    }

    protected override void MouseExited()
    {
        base.MouseExited();
        _hovered = false;
        _pressed = false;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _pressed = true;
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (_pressed && _hovered)
        {
            UserInterfaceManager.ClickSound();
            OnPressed?.Invoke();
        }

        _pressed = false;
        args.Handle();
    }

    protected override void UIScaleChanged()
    {
        base.UIScaleChanged();
        UpdateFont();
        InvalidateMeasure();
    }
}
