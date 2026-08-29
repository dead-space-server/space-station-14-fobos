// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets.Colorspace;
using Content.Client.Stylesheets.Palette;
using Content.Shared.DeadSpace.CCCCVars;

namespace Content.Client.DeadSpace.Stylesheets;

/// <summary>
/// Shared DS14 UI colors. A theme switch replaces this palette before rebuilding
/// the content stylesheet, while direct runtime users always resolve the active colors.
/// </summary>
public static class DeadSpaceStylePalette
{
    public const float NeutralLightnessOffset = -0.002f;

    private static readonly ThemePalette Dark = CreateDark();
    private static readonly ThemePalette Light = CreateLight();
    private static readonly ThemePalette Classic = CreateClassic();
    private static ThemePalette _current = Dark;

    public static string CurrentTheme { get; private set; } = CCCCVars.InterfaceStyleDark;
    public static bool ClassicChrome => _current.ClassicChrome;
    public static bool LightChrome => ReferenceEquals(_current, Light);

    public static Color Surface => _current.Surface;
    public static Color SurfaceDark => _current.SurfaceDark;
    public static Color SurfaceFlat => _current.SurfaceFlat;
    public static Color SurfaceHeader => _current.SurfaceHeader;
    public static Color SurfaceInset => _current.SurfaceInset;
    public static Color SurfaceStatus => _current.SurfaceStatus;
    public static Color SurfacePopup => _current.SurfacePopup;
    public static Color SurfaceIcon => _current.SurfaceIcon;
    public static Color SurfaceTabs => _current.SurfaceTabs;
    public static Color SurfaceTabActive => _current.SurfaceTabActive;
    public static Color SurfaceTabInactive => _current.SurfaceTabInactive;
    public static Color ModalScrim => _current.ModalScrim;

    public static Color Control => _current.Control;
    public static Color ControlHover => _current.ControlHover;
    public static Color ControlPressed => _current.ControlPressed;
    public static Color ControlDisabled => _current.ControlDisabled;
    public static Color Action => _current.Action;
    public static Color ActionHover => _current.ActionHover;
    public static Color ActionPressed => _current.ActionPressed;
    public static Color ActionDisabled => _current.ActionDisabled;
    public static Color ListItem => _current.ListItem;
    public static Color ListItemAlternate => _current.ListItemAlternate;
    public static Color ListItemHover => _current.ListItemHover;
    public static Color ListItemPressed => _current.ListItemPressed;
    public static Color Input => _current.Input;

    public static Color Border => _current.Border;
    public static Color BorderDark => _current.BorderDark;
    public static Color BorderHeader => _current.BorderHeader;
    public static Color BorderInset => _current.BorderInset;
    public static Color BorderControl => _current.BorderControl;
    public static Color BorderDisabled => _current.BorderDisabled;
    public static Color BorderIcon => _current.BorderIcon;
    public static Color BorderTabActive => _current.BorderTabActive;
    public static Color BorderTabInactive => _current.BorderTabInactive;
    public static Color HoverOutline => _current.HoverOutline;
    public static Color PressedOutline => _current.PressedOutline;

    public static Color CyanDim => _current.CyanDim;
    public static Color Cyan => _current.Cyan;
    public static Color CyanBright => _current.CyanBright;
    public static Color CyanSelection => _current.CyanSelection;
    public static Color Amber => _current.Amber;
    public static Color AccentDim => _current.AccentDim;

    public static Color Text => _current.Text;
    public static Color TextInactive => _current.TextInactive;
    public static Color TextMuted => _current.TextMuted;
    public static Color TextPlaceholder => _current.TextPlaceholder;

    public static Color Positive => _current.Positive;
    public static Color PositiveHover => _current.PositiveHover;
    public static Color PositivePressed => _current.PositivePressed;
    public static Color PositiveBorder => _current.PositiveBorder;
    public static Color PositiveBorderHover => _current.PositiveBorderHover;
    public static Color PositiveBorderPressed => _current.PositiveBorderPressed;
    public static Color Negative => _current.Negative;
    public static Color NegativeHover => _current.NegativeHover;
    public static Color NegativeStrong => _current.NegativeStrong;
    public static Color NegativeStrongHover => _current.NegativeStrongHover;
    public static Color NegativeBorder => _current.NegativeBorder;
    public static Color NegativeBorderStrong => _current.NegativeBorderStrong;
    public static Color NegativeBorderHover => _current.NegativeBorderHover;
    public static Color Warning => _current.Warning;
    public static Color WarningControl => _current.WarningControl;
    public static Color WarningControlHover => _current.WarningControlHover;
    public static Color WarningControlPressed => _current.WarningControlPressed;
    public static Color WarningBorder => _current.WarningBorder;
    public static Color WarningBorderHover => _current.WarningBorderHover;

    public static ColorPalette PrimaryPalette => _current.PrimaryPalette;
    public static ColorPalette SecondaryPalette => _current.SecondaryPalette;

    /// <summary>
    /// Selects a canonical palette. Invalid archived values safely fall back to the current dark style.
    /// </summary>
    public static bool TrySetTheme(string theme)
    {
        if (string.Equals(theme, CCCCVars.InterfaceStyleLight, StringComparison.OrdinalIgnoreCase))
        {
            _current = Light;
            CurrentTheme = CCCCVars.InterfaceStyleLight;
            return true;
        }

        if (string.Equals(theme, CCCCVars.InterfaceStyleClassic, StringComparison.OrdinalIgnoreCase))
        {
            _current = Classic;
            CurrentTheme = CCCCVars.InterfaceStyleClassic;
            return true;
        }

        _current = Dark;
        CurrentTheme = CCCCVars.InterfaceStyleDark;
        return string.Equals(theme, CCCCVars.InterfaceStyleDark, StringComparison.OrdinalIgnoreCase);
    }

    private static ThemePalette CreateDark()
    {
        var palette = new ThemePalette
        {
            Surface = Neutral("#1B1F25"),
            SurfaceDark = Neutral("#0F1318"),
            SurfaceFlat = Neutral("#20252C"),
            SurfaceHeader = Neutral("#252B33"),
            SurfaceInset = Neutral("#0C1015"),
            SurfaceStatus = Neutral("#181E25"),
            SurfacePopup = Neutral("#11161C"),
            SurfaceIcon = Neutral("#06080B"),
            SurfaceTabs = Neutral("#12171D"),
            SurfaceTabActive = Neutral("#272E38"),
            SurfaceTabInactive = Neutral("#12171D"),
            ModalScrim = Color.FromHex("#000000AA"),
            Control = Neutral("#1A232DF2"),
            ControlHover = Neutral("#263440F6"),
            ControlPressed = Neutral("#314350F8"),
            ControlDisabled = Neutral("#11161CCF"),
            Action = Neutral("#1B252FF4"),
            ActionHover = Neutral("#283744F7"),
            ActionPressed = Neutral("#354856F8"),
            ActionDisabled = Neutral("#111820CF"),
            ListItem = Neutral("#202B35F2"),
            ListItemAlternate = Neutral("#273440F2"),
            ListItemHover = Neutral("#30414FF6"),
            ListItemPressed = Neutral("#3A4E5EF8"),
            Input = Neutral("#151C24F8"),
            Border = Color.FromHex("#3A414A"),
            BorderDark = Color.FromHex("#252B32"),
            BorderHeader = Color.FromHex("#574936"),
            BorderInset = Color.FromHex("#222931"),
            BorderControl = Color.FromHex("#343C45"),
            BorderDisabled = Color.FromHex("#242A31"),
            BorderIcon = Color.FromHex("#303840"),
            BorderTabActive = Color.FromHex("#B98B52"),
            BorderTabInactive = Color.Transparent,
            HoverOutline = Color.FromHex("#B98B52"),
            PressedOutline = Color.FromHex("#D6A35F"),
            CyanDim = Color.FromHex("#1D5B73"),
            Cyan = Color.FromHex("#1D8BAD"),
            CyanBright = Color.FromHex("#2EA7D0"),
            CyanSelection = Color.FromHex("#1D7E9D88"),
            Amber = Palettes.Gold.Text,
            AccentDim = Color.FromHex("#514431"),
            Text = Color.FromHex("#ECEEF1"),
            TextInactive = Color.FromHex("#B5BAC1"),
            TextMuted = Color.FromHex("#9CA3AB"),
            TextPlaceholder = Color.FromHex("#7F8791"),
            Positive = Color.FromHex("#1D4B2EF4"),
            PositiveHover = Color.FromHex("#245F39F8"),
            PositivePressed = Color.FromHex("#2B7A40F8"),
            PositiveBorder = Color.FromHex("#2EA043"),
            PositiveBorderHover = Color.FromHex("#3FB950"),
            PositiveBorderPressed = Color.FromHex("#56D364"),
            Negative = Color.FromHex("#3B1C23F2"),
            NegativeHover = Color.FromHex("#51242CF6"),
            NegativeStrong = Color.FromHex("#652A33F4"),
            NegativeStrongHover = Color.FromHex("#7A303AF8"),
            NegativeBorder = Color.FromHex("#9D3F49"),
            NegativeBorderStrong = Color.FromHex("#C44B55"),
            NegativeBorderHover = Color.FromHex("#F85149"),
            Warning = Color.FromHex("#947300"),
            WarningControl = Color.FromHex("#4A2A16F4"),
            WarningControlHover = Color.FromHex("#66361BF8"),
            WarningControlPressed = Color.FromHex("#84451FF8"),
            WarningBorder = Color.FromHex("#D86F32"),
            WarningBorderHover = Color.FromHex("#F0883E"),
        };

        return palette with
        {
            PrimaryPalette = ColorPalette.FromHexBase(
                "#6A573F",
                lightnessShift: 0.05f,
                chromaShift: 0.003f,
                element: palette.Control,
                background: palette.SurfaceDark,
                text: palette.Text),
            SecondaryPalette = ColorPalette.FromHexBase(
                "#34373B",
                lightnessShift: 0.05f,
                element: palette.SurfaceHeader,
                background: palette.Surface,
                text: palette.TextMuted),
        };
    }

    private static ThemePalette CreateLight()
    {
        var palette = new ThemePalette
        {
            Surface = Color.FromHex("#DAD9D6"),
            SurfaceDark = Color.FromHex("#C7C8C9"),
            SurfaceFlat = Color.FromHex("#E7E5E1"),
            SurfaceHeader = Color.FromHex("#D0CCC5"),
            SurfaceInset = Color.FromHex("#C0C3C6"),
            SurfaceStatus = Color.FromHex("#D1D3D4"),
            SurfacePopup = Color.FromHex("#F0EFEC"),
            SurfaceIcon = Color.FromHex("#4C535BFA"),
            SurfaceTabs = Color.FromHex("#CFD0D0"),
            SurfaceTabActive = Color.FromHex("#F2EEE7"),
            SurfaceTabInactive = Color.FromHex("#CFD0D0"),
            ModalScrim = Color.FromHex("#10131888"),
            Control = Color.FromHex("#E2E5E8FA"),
            ControlHover = Color.FromHex("#EEE4D6FC"),
            ControlPressed = Color.FromHex("#DEC7A5FC"),
            ControlDisabled = Color.FromHex("#CDD0D2"),
            Action = Color.FromHex("#DDE2E6FC"),
            ActionHover = Color.FromHex("#EEE2D2FD"),
            ActionPressed = Color.FromHex("#D9BF9AFD"),
            ActionDisabled = Color.FromHex("#C9CDD0D9"),
            ListItem = Color.FromHex("#E3E6E8FC"),
            ListItemAlternate = Color.FromHex("#D8DDE0FC"),
            ListItemHover = Color.FromHex("#EEE3D5FD"),
            ListItemPressed = Color.FromHex("#DCC39FFD"),
            Input = Color.FromHex("#F5F6F7FE"),
            Border = Color.FromHex("#7A8087"),
            BorderDark = Color.FromHex("#9AA0A6"),
            BorderHeader = Color.FromHex("#9A7A51"),
            BorderInset = Color.FromHex("#A5A9AD"),
            BorderControl = Color.FromHex("#8D949B"),
            BorderDisabled = Color.FromHex("#B6BABE"),
            BorderIcon = Color.FromHex("#6C737A"),
            BorderTabActive = Color.FromHex("#96672E"),
            BorderTabInactive = Color.Transparent,
            HoverOutline = Color.FromHex("#96672E"),
            PressedOutline = Color.FromHex("#714514"),
            CyanDim = Color.FromHex("#39758A"),
            Cyan = Color.FromHex("#176D89"),
            CyanBright = Color.FromHex("#0E789A"),
            CyanSelection = Color.FromHex("#1D7E9D55"),
            Amber = Color.FromHex("#76501E"),
            AccentDim = Color.FromHex("#9B7D55"),
            Text = Color.FromHex("#20242A"),
            TextInactive = Color.FromHex("#3E454D"),
            TextMuted = Color.FromHex("#5B636C"),
            TextPlaceholder = Color.FromHex("#747C84"),
            Positive = Color.FromHex("#CDE4D3F8"),
            PositiveHover = Color.FromHex("#B8D9C0FC"),
            PositivePressed = Color.FromHex("#A2CEADFC"),
            PositiveBorder = Color.FromHex("#347E48"),
            PositiveBorderHover = Color.FromHex("#286D3C"),
            PositiveBorderPressed = Color.FromHex("#1E5C31"),
            Negative = Color.FromHex("#EED4D8F8"),
            NegativeHover = Color.FromHex("#E4BBC2FC"),
            NegativeStrong = Color.FromHex("#E5BAC1F8"),
            NegativeStrongHover = Color.FromHex("#DDA4AEFC"),
            NegativeBorder = Color.FromHex("#9D3F49"),
            NegativeBorderStrong = Color.FromHex("#A73541"),
            NegativeBorderHover = Color.FromHex("#842C36"),
            Warning = Color.FromHex("#D6B65A"),
            WarningControl = Color.FromHex("#EFDDC8F8"),
            WarningControlHover = Color.FromHex("#E6C9A8FC"),
            WarningControlPressed = Color.FromHex("#DAB78EFC"),
            WarningBorder = Color.FromHex("#9A552C"),
            WarningBorderHover = Color.FromHex("#7F421F"),
        };

        return palette with
        {
            PrimaryPalette = new ColorPalette(
                Color.FromHex("#8A6A42"),
                0.05f,
                0.003f,
                palette.Control,
                palette.ControlHover,
                palette.ControlPressed,
                palette.ControlDisabled,
                palette.Surface,
                palette.SurfaceFlat,
                palette.SurfaceDark,
                palette.Text,
                palette.TextMuted),
            SecondaryPalette = new ColorPalette(
                Color.FromHex("#9A9DA0"),
                0.05f,
                0f,
                palette.SurfaceHeader,
                palette.SurfaceFlat,
                palette.SurfaceDark,
                palette.ControlDisabled,
                palette.Surface,
                palette.SurfaceFlat,
                palette.SurfaceDark,
                palette.TextMuted,
                palette.TextPlaceholder),
        };
    }

    private static ThemePalette CreateClassic()
    {
        return new ThemePalette
        {
            ClassicChrome = true,
            // The legacy option is the pre-7074b42 UI: ordinary Wizards/Nanotrasen palettes, not the
            // later cyan DS14 menu sheetlet. These values only bridge layout classes that did not exist then.
            Surface = Palettes.Slate.Background,
            SurfaceDark = Color.FromHex("#25252A"),
            SurfaceFlat = Palettes.Slate.Background,
            SurfaceHeader = Palettes.Slate.Element,
            SurfaceInset = Palettes.Slate.BackgroundDark,
            SurfaceStatus = Palettes.Slate.Background,
            SurfacePopup = Palettes.Slate.BackgroundDark,
            SurfaceIcon = Color.Black,
            SurfaceTabs = Palettes.Slate.Background,
            SurfaceTabActive = Palettes.Slate.Element,
            SurfaceTabInactive = Palettes.Slate.Background,
            ModalScrim = Color.FromHex("#000000AA"),
            Control = Palettes.Navy.Element,
            ControlHover = Palettes.Navy.HoveredElement,
            ControlPressed = Palettes.Navy.PressedElement,
            ControlDisabled = Palettes.Navy.DisabledElement,
            Action = Palettes.Navy.Element,
            ActionHover = Palettes.Navy.HoveredElement,
            ActionPressed = Palettes.Navy.PressedElement,
            ActionDisabled = Palettes.Navy.DisabledElement,
            ListItem = Palettes.Navy.Element,
            ListItemAlternate = Palettes.Slate.Element,
            ListItemHover = Palettes.Navy.HoveredElement,
            ListItemPressed = Palettes.Navy.PressedElement,
            Input = Palettes.Navy.BackgroundDark,
            Border = Color.FromHex("#525252"),
            BorderDark = Color.FromHex("#3F3F43"),
            BorderHeader = Color.FromHex("#525252"),
            BorderInset = Color.FromHex("#3F3F43"),
            BorderControl = Color.FromHex("#525252"),
            BorderDisabled = Color.FromHex("#38383D"),
            BorderIcon = Color.FromHex("#525252"),
            BorderTabActive = Palettes.Slate.HoveredElement,
            BorderTabInactive = Color.Transparent,
            HoverOutline = Palettes.Navy.HoveredElement,
            PressedOutline = Palettes.Navy.PressedElement,
            CyanDim = Color.FromHex("#75838E"),
            Cyan = Color.FromHex("#789B8C"),
            CyanBright = Color.FromHex("#ACBAC6"),
            CyanSelection = Color.FromHex("#789B8C88"),
            Amber = Palettes.Gold.Text,
            AccentDim = Color.FromHex("#525252"),
            Text = Color.White,
            TextInactive = Color.FromHex("#99A7B3"),
            TextMuted = Color.FromHex("#757575"),
            TextPlaceholder = Color.FromHex("#5A5A5A"),
            Positive = Palettes.Green.Element,
            PositiveHover = Palettes.Green.HoveredElement,
            PositivePressed = Palettes.Green.PressedElement,
            PositiveBorder = Palettes.Green.Element,
            PositiveBorderHover = Palettes.Green.HoveredElement,
            PositiveBorderPressed = Palettes.Green.PressedElement,
            Negative = Palettes.Red.Element,
            NegativeHover = Palettes.Red.HoveredElement,
            NegativeStrong = Palettes.Red.Element,
            NegativeStrongHover = Palettes.Red.HoveredElement,
            NegativeBorder = Palettes.Red.Element,
            NegativeBorderStrong = Palettes.Red.PressedElement,
            NegativeBorderHover = Palettes.Red.HoveredElement,
            Warning = Palettes.Amber.Background,
            WarningControl = Palettes.Amber.Element,
            WarningControlHover = Palettes.Amber.HoveredElement,
            WarningControlPressed = Palettes.Amber.PressedElement,
            WarningBorder = Palettes.Amber.Element,
            WarningBorderHover = Palettes.Amber.HoveredElement,
            PrimaryPalette = Palettes.Navy,
            SecondaryPalette = Palettes.Slate,
        };
    }

    private static Color Neutral(string hex)
    {
        return Color.FromHex(hex).NudgeLightness(NeutralLightnessOffset);
    }

    private sealed record ThemePalette
    {
        public bool ClassicChrome { get; init; }
        public Color Surface { get; init; }
        public Color SurfaceDark { get; init; }
        public Color SurfaceFlat { get; init; }
        public Color SurfaceHeader { get; init; }
        public Color SurfaceInset { get; init; }
        public Color SurfaceStatus { get; init; }
        public Color SurfacePopup { get; init; }
        public Color SurfaceIcon { get; init; }
        public Color SurfaceTabs { get; init; }
        public Color SurfaceTabActive { get; init; }
        public Color SurfaceTabInactive { get; init; }
        public Color ModalScrim { get; init; }
        public Color Control { get; init; }
        public Color ControlHover { get; init; }
        public Color ControlPressed { get; init; }
        public Color ControlDisabled { get; init; }
        public Color Action { get; init; }
        public Color ActionHover { get; init; }
        public Color ActionPressed { get; init; }
        public Color ActionDisabled { get; init; }
        public Color ListItem { get; init; }
        public Color ListItemAlternate { get; init; }
        public Color ListItemHover { get; init; }
        public Color ListItemPressed { get; init; }
        public Color Input { get; init; }
        public Color Border { get; init; }
        public Color BorderDark { get; init; }
        public Color BorderHeader { get; init; }
        public Color BorderInset { get; init; }
        public Color BorderControl { get; init; }
        public Color BorderDisabled { get; init; }
        public Color BorderIcon { get; init; }
        public Color BorderTabActive { get; init; }
        public Color BorderTabInactive { get; init; }
        public Color HoverOutline { get; init; }
        public Color PressedOutline { get; init; }
        public Color CyanDim { get; init; }
        public Color Cyan { get; init; }
        public Color CyanBright { get; init; }
        public Color CyanSelection { get; init; }
        public Color Amber { get; init; }
        public Color AccentDim { get; init; }
        public Color Text { get; init; }
        public Color TextInactive { get; init; }
        public Color TextMuted { get; init; }
        public Color TextPlaceholder { get; init; }
        public Color Positive { get; init; }
        public Color PositiveHover { get; init; }
        public Color PositivePressed { get; init; }
        public Color PositiveBorder { get; init; }
        public Color PositiveBorderHover { get; init; }
        public Color PositiveBorderPressed { get; init; }
        public Color Negative { get; init; }
        public Color NegativeHover { get; init; }
        public Color NegativeStrong { get; init; }
        public Color NegativeStrongHover { get; init; }
        public Color NegativeBorder { get; init; }
        public Color NegativeBorderStrong { get; init; }
        public Color NegativeBorderHover { get; init; }
        public Color Warning { get; init; }
        public Color WarningControl { get; init; }
        public Color WarningControlHover { get; init; }
        public Color WarningControlPressed { get; init; }
        public Color WarningBorder { get; init; }
        public Color WarningBorderHover { get; init; }
        public ColorPalette PrimaryPalette { get; init; } = null!;
        public ColorPalette SecondaryPalette { get; init; } = null!;
    }
}
