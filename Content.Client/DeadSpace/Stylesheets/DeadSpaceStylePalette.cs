// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;

namespace Content.Client.DeadSpace.Stylesheets;

/// <summary>
/// Shared DS14 UI colors. Large surfaces stay close to black while warm hover
/// outlines, readable text and semantic colors retain their own contrast.
/// </summary>
public static class DeadSpaceStylePalette
{
    public const float NeutralLightnessOffset = -0.002f;

    public static readonly Color Surface = Neutral("#1B1F25");
    public static readonly Color SurfaceDark = Neutral("#0F1318");
    public static readonly Color SurfaceFlat = Neutral("#20252C");
    public static readonly Color SurfaceHeader = Neutral("#252B33");
    public static readonly Color SurfaceInset = Neutral("#0C1015");
    public static readonly Color SurfaceStatus = Neutral("#181E25");
    public static readonly Color SurfacePopup = Neutral("#11161C");
    public static readonly Color SurfaceIcon = Neutral("#06080B");
    public static readonly Color SurfaceTabs = Neutral("#12171D");
    public static readonly Color SurfaceTabActive = Neutral("#272E38");
    public static readonly Color SurfaceTabInactive = Neutral("#12171D");
    public static readonly Color ModalScrim = Color.FromHex("#000000AA");

    public static readonly Color Control = Neutral("#1A232DF2");
    public static readonly Color ControlHover = Neutral("#263440F6");
    public static readonly Color ControlPressed = Neutral("#314350F8");
    public static readonly Color ControlDisabled = Neutral("#11161CCF");
    public static readonly Color Action = Neutral("#1B252FF4");
    public static readonly Color ActionHover = Neutral("#283744F7");
    public static readonly Color ActionPressed = Neutral("#354856F8");
    public static readonly Color ActionDisabled = Neutral("#111820CF");
    public static readonly Color ListItem = Neutral("#202B35F2");
    public static readonly Color ListItemAlternate = Neutral("#273440F2");
    public static readonly Color ListItemHover = Neutral("#30414FF6");
    public static readonly Color ListItemPressed = Neutral("#3A4E5EF8");
    public static readonly Color Input = Neutral("#151C24F8");

    public static readonly Color Border = Color.FromHex("#3A414A");
    public static readonly Color BorderDark = Color.FromHex("#252B32");
    public static readonly Color BorderHeader = Color.FromHex("#574936");
    public static readonly Color BorderInset = Color.FromHex("#222931");
    public static readonly Color BorderControl = Color.FromHex("#343C45");
    public static readonly Color BorderDisabled = Color.FromHex("#242A31");
    public static readonly Color BorderIcon = Color.FromHex("#303840");
    public static readonly Color BorderTabActive = Color.FromHex("#B98B52");
    public static readonly Color BorderTabInactive = Color.Transparent;
    public static readonly Color HoverOutline = Color.FromHex("#B98B52");
    public static readonly Color PressedOutline = Color.FromHex("#D6A35F");

    public static readonly Color CyanDim = Color.FromHex("#1D5B73");
    public static readonly Color Cyan = Color.FromHex("#1D8BAD");
    public static readonly Color CyanBright = Color.FromHex("#2EA7D0");
    public static readonly Color CyanSelection = Color.FromHex("#1D7E9D88");
    public static readonly Color Amber = Palettes.Gold.Text;
    public static readonly Color AccentDim = Color.FromHex("#514431");

    public static readonly Color Text = Color.FromHex("#ECEEF1");
    public static readonly Color TextInactive = Color.FromHex("#B5BAC1");
    public static readonly Color TextMuted = Color.FromHex("#9CA3AB");
    public static readonly Color TextPlaceholder = Color.FromHex("#7F8791");

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
        "#6A573F",
        lightnessShift: 0.05f,
        chromaShift: 0.003f,
        element: Control,
        background: SurfaceDark,
        text: Text);

    public static readonly ColorPalette SecondaryPalette = ColorPalette.FromHexBase(
        "#34373B",
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
