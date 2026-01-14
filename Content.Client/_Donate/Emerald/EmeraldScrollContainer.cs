// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

public sealed class EmeraldScrollContainer : Control
{
    private Control? _content;
    private float _scrollOffset;
    private float _maxScroll;

    private bool _draggingScrollbar;
    private float _dragStartY;
    private float _dragStartOffset;
    private bool _hoveringScrollbar;

    private readonly Color _scrollbarTrackColor = Color.FromHex("#1a0f2e");
    private readonly Color _scrollbarThumbColor = Color.FromHex("#6d5a8a");
    private readonly Color _scrollbarHoverColor = Color.FromHex("#a589c9");

    private const float BaseScrollbarWidth = 12f;
    private const float BaseMinGrabberHeight = 30f;
    private const float BaseScrollbarPadding = 2f;

    public EmeraldScrollContainer()
    {
        RectClipContent = true;
        MouseFilter = MouseFilterMode.Pass;
    }

    private float ScrollbarWidth => BaseScrollbarWidth * UIScale;
    private float MinGrabberHeight => BaseMinGrabberHeight * UIScale;
    private float ScrollbarPadding => BaseScrollbarPadding * UIScale;

    public void SetContent(Control child)
    {
        if (_content != null)
            RemoveChild(_content);

        _content = child;
        AddChild(child);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (_content == null)
            return Vector2.Zero;

        var contentWidth = Math.Max(0, availableSize.X - ScrollbarWidth - ScrollbarPadding);
        _content.Measure(new Vector2(contentWidth, float.PositiveInfinity));

        return availableSize;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        if (_content == null)
            return finalSize;

        var contentHeight = _content.DesiredSize.Y;
        var viewportHeight = Math.Max(1, finalSize.Y);

        _maxScroll = Math.Max(0, contentHeight - viewportHeight);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScroll);

        var contentWidth = Math.Max(0, finalSize.X - ScrollbarWidth - ScrollbarPadding);
        var contentBox = new UIBox2(0, -_scrollOffset, contentWidth, contentHeight - _scrollOffset);

        _content.Arrange(contentBox);
        return finalSize;
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        if (_maxScroll <= 0)
            return;

        var delta = args.Delta.Y * 30f * UIScale;
        _scrollOffset = Math.Clamp(_scrollOffset - delta, 0, _maxScroll);

        InvalidateArrange();
        args.Handle();
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != Robust.Shared.Input.EngineKeyFunctions.UIClick)
            return;

        var scrollbarRect = GetScrollbarRect();
        if (!scrollbarRect.Contains(args.RelativePosition))
            return;

        var grabberRect = GetGrabberRect();
        if (grabberRect.Contains(args.RelativePosition))
        {
            _draggingScrollbar = true;
            _dragStartY = args.RelativePosition.Y;
            _dragStartOffset = _scrollOffset;
        }
        else
        {
            var clickY = args.RelativePosition.Y;
            var grabberCenter = grabberRect.Top + grabberRect.Height / 2;

            if (clickY < grabberCenter)
                _scrollOffset = Math.Max(0, _scrollOffset - PixelSize.Y * 0.5f);
            else
                _scrollOffset = Math.Min(_maxScroll, _scrollOffset + PixelSize.Y * 0.5f);

            InvalidateArrange();
        }

        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == Robust.Shared.Input.EngineKeyFunctions.UIClick)
            _draggingScrollbar = false;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        var scrollbarRect = GetScrollbarRect();
        _hoveringScrollbar = scrollbarRect.Contains(args.RelativePosition);

        if (!_draggingScrollbar)
            return;

        var deltaY = args.RelativePosition.Y - _dragStartY;
        var grabberHeight = GetGrabberHeight();
        var trackHeight = Math.Max(1, PixelSize.Y - grabberHeight);

        if (_maxScroll > 0)
        {
            var scrollDelta = deltaY / trackHeight * _maxScroll;
            _scrollOffset = Math.Clamp(_dragStartOffset + scrollDelta, 0, _maxScroll);
            InvalidateArrange();
        }
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (_maxScroll <= 0 || PixelSize.X < ScrollbarWidth || PixelSize.Y < 1)
            return;

        var scrollbarRect = GetScrollbarRect();
        if (scrollbarRect.Width <= 0 || scrollbarRect.Height <= 0)
            return;

        handle.DrawRect(scrollbarRect, _scrollbarTrackColor);

        var grabberRect = GetGrabberRect();
        if (grabberRect.Width > 0 && grabberRect.Height > 0)
        {
            var thumbColor = _draggingScrollbar ? _scrollbarHoverColor :
                             _hoveringScrollbar ? _scrollbarHoverColor.WithAlpha(0.8f) :
                             _scrollbarThumbColor;

            handle.DrawRect(grabberRect, thumbColor);

            var highlightWidth = 2f * UIScale;
            var highlightRect = new UIBox2(
                grabberRect.Left,
                grabberRect.Top,
                grabberRect.Left + highlightWidth,
                grabberRect.Bottom
            );
            handle.DrawRect(highlightRect, Color.FromHex("#d4c5e8").WithAlpha(0.3f));
        }
    }

    private UIBox2 GetScrollbarRect()
    {
        var left = Math.Max(0, PixelSize.X - ScrollbarWidth);
        return new UIBox2(left, 0, PixelSize.X, Math.Max(0, PixelSize.Y));
    }

    private UIBox2 GetGrabberRect()
    {
        var scrollbarRect = GetScrollbarRect();
        var grabberHeight = GetGrabberHeight();
        var trackHeight = Math.Max(1, PixelSize.Y - grabberHeight);

        var grabberY = _maxScroll > 0 ? _scrollOffset / _maxScroll * trackHeight : 0;

        var grabberPadding = 2f * UIScale;
        var left = scrollbarRect.Left + grabberPadding;
        var right = scrollbarRect.Right - grabberPadding;
        var top = Math.Max(0, grabberY);
        var bottom = Math.Max(top + 1, grabberY + grabberHeight);

        return new UIBox2(left, top, right, bottom);
    }

    private float GetGrabberHeight()
    {
        if (_content == null || _maxScroll <= 0 || PixelSize.Y <= 0)
            return PixelSize.Y;

        var contentHeight = Math.Max(1, _content.DesiredSize.Y);
        var ratio = PixelSize.Y / contentHeight;
        var height = PixelSize.Y * ratio;
        return Math.Clamp(height, MinGrabberHeight, PixelSize.Y);
    }
}
