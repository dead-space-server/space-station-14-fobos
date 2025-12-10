// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._Donate.Emerald;

[Virtual]
public class EmeraldPanel : Control
{
    private readonly Color _bgColor = Color.FromHex("#1a0f2e");
    private readonly Color _borderColor = Color.FromHex("#4a3a6a");

    public EmeraldPanel()
    {
        MouseFilter = MouseFilterMode.Stop;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var rect = new UIBox2(0, 0, PixelSize.X, PixelSize.Y);

        handle.DrawRect(rect, _bgColor.WithAlpha(0.9f));

        DrawBorder(handle, rect);
    }

    private void DrawBorder(DrawingHandleScreen handle, UIBox2 rect)
    {
        handle.DrawLine(rect.TopLeft, rect.TopRight, _borderColor);
        handle.DrawLine(rect.TopRight, rect.BottomRight, _borderColor);
        handle.DrawLine(rect.BottomRight, rect.BottomLeft, _borderColor);
        handle.DrawLine(rect.BottomLeft, rect.TopLeft, _borderColor);
    }
}
