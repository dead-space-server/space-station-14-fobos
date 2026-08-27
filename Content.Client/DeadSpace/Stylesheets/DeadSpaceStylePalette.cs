// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;

namespace Content.Client.DeadSpace.Stylesheets;

/// <summary>
/// Shared DS14 UI colors. Neutral surfaces are shifted by -0.01 OKLab lightness;
/// readable text, accents and semantic colors retain their original values.
/// </summary>
public static class DeadSpaceStylePalette
{
    public const float NeutralLightnessOffset = -0.01f;

    public static readonly Color Surface = Neutral("#1A1E25");
    public static readonly Color SurfaceDark = Neutral("#12161D");
    public static readonly Color SurfaceFlat = Neutral("#171A20");
    public static readonly Color SurfaceHeader = Neutral("#202631");
    public static readonly Color SurfaceInset = Neutral("#10141B");
    public static readonly Color SurfaceStatus = Neutral("#121821");
    public static readonly Color SurfacePopup = Neutral("#10161F");
    public static readonly Color SurfaceIcon = Neutral("#070B10");
    public static readonly Color SurfaceTabs = Neutral("#12161D");
    public static readonly Color SurfaceTabActive = Neutral("#48536D");
    public static readonly Color SurfaceTabInactive = Neutral("#1D2330");
    public static readonly Color ModalScrim = Color.FromHex("#000000AA");

    public static readonly Color Control = Neutral("#111923F0");
    public static readonly Color ControlHover = Neutral("#162638F4");
    public static readonly Color ControlPressed = Neutral("#18344AF5");
    public static readonly Color ControlDisabled = Neutral("#10161FC8");
    public static readonly Color Action = Neutral("#111A25F2");
    public static readonly Color ActionHover = Neutral("#162A40F6");
    public static readonly Color ActionPressed = Neutral("#193A54F6");
    public static readonly Color ActionDisabled = Neutral("#101720C8");
    public static readonly Color ListItem = Neutral("#121821F0");
    public static readonly Color ListItemAlternate = Neutral("#17202BF0");
    public static readonly Color ListItemHover = Neutral("#17283AF4");
    public static readonly Color ListItemPressed = Neutral("#1B3448F5");
    public static readonly Color Input = Neutral("#0D1219F6");

    public static readonly Color Border = Color.FromHex("#4C5666");
    public static readonly Color BorderDark = Color.FromHex("#394352");
    public static readonly Color BorderHeader = Color.FromHex("#5D6A7C");
    public static readonly Color BorderInset = Color.FromHex("#3F4958");
    public static readonly Color BorderControl = Color.FromHex("#2D4757");
    public static readonly Color BorderDisabled = Color.FromHex("#293844");
    public static readonly Color BorderIcon = Color.FromHex("#263842");
    public static readonly Color BorderTabActive = Color.FromHex("#68758E");
    public static readonly Color BorderTabInactive = Color.FromHex("#374252");

    public static readonly Color CyanDim = Color.FromHex("#1D5B73");
    public static readonly Color Cyan = Color.FromHex("#1D8BAD");
    public static readonly Color CyanBright = Color.FromHex("#2EA7D0");
    public static readonly Color CyanSelection = Color.FromHex("#1D7E9D88");
    public static readonly Color Amber = Palettes.Gold.Text;
    public static readonly Color AccentDim = Color.FromHex("#3B4653");

    public static readonly Color Text = Color.FromHex("#F1F3F6");
    public static readonly Color TextInactive = Color.FromHex("#AEB6C2");
    public static readonly Color TextMuted = Color.FromHex("#9BA6AD");
    public static readonly Color TextPlaceholder = Color.FromHex("#7A8590");

    // Semantic values intentionally remain unchanged by the neutral lightness adjustment.
    public static readonly Color Positive = Color.FromHex("#1D4B2EF4");
    public static readonly Color PositiveHover = Color.FromHex("#245F39F8");
    public static readonly Color PositivePressed = Color.FromHex("#2B7A40F8");
    public static readonly Color PositiveBorder = Color.FromHex("#2EA043");
    public static readonly Color PositiveBorderHover = Color.FromHex("#3FB950");
    public static readonly Color PositiveBorderPressed = Color.FromHex("#56D364");
    public static readonly Color Negative = Color.FromHex("#3B1C23F2");
    public static readonly Color NegativeHover = Color.FromHex("#51242CF6");
    public static readonly Color NegativeStrong = Color.FromHex("#652A33F4");
    public static readonly Color NegativeStrongHover = Color.FromHex("#7A303AF8");
    public static readonly Color NegativeBorder = Color.FromHex("#9D3F49");
    public static readonly Color NegativeBorderStrong = Color.FromHex("#C44B55");
    public static readonly Color NegativeBorderHover = Color.FromHex("#F85149");
    public static readonly Color Warning = Color.FromHex("#947300");
    public static readonly Color WarningControl = Color.FromHex("#4A2A16F4");
    public static readonly Color WarningControlHover = Color.FromHex("#66361BF8");
    public static readonly Color WarningControlPressed = Color.FromHex("#84451FF8");
    public static readonly Color WarningBorder = Color.FromHex("#D86F32");
    public static readonly Color WarningBorderHover = Color.FromHex("#F0883E");

    public static readonly ColorPalette PrimaryPalette = ColorPalette.FromHexBase(
        "#42586A",
        lightnessShift: 0.05f,
        chromaShift: 0.0045f,
        element: Control,
        background: SurfaceDark,
        text: Text);

    public static readonly ColorPalette SecondaryPalette = ColorPalette.FromHexBase(
        "#4A5360",
        lightnessShift: 0.05f,
        element: SurfaceHeader,
        // Chat and other large secondary surfaces need one lighter neutral step than deep insets.
        background: Surface,
        text: TextMuted);

    private static Color Neutral(string hex)
    {
        return Color.FromHex(hex).NudgeLightness(NeutralLightnessOffset);
    }
}
