// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets;
using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.DeadSpace.Stylesheets;

[CommonSheetlet]
public sealed class DeadSpaceMenuSheetlet : Sheetlet<PalettedStylesheet>
{
    public const string Shell = "DS14MenuShell";
    public const string TopShell = "DS14MenuTopShell";
    public const string Panel = "DS14MenuPanel";
    public const string PanelDark = "DS14MenuPanelDark";
    public const string Header = "DS14MenuHeader";
    public const string Inset = "DS14MenuInset";
    public const string RoundStatus = "DS14MenuRoundStatus";
    public const string Tabs = "DS14MenuTabs";
    public const string Accent = "DS14MenuAccent";
    public const string AccentDim = "DS14MenuAccentDim";
    public const string ActionButton = "DS14MenuAction";
    public const string TopButton = "DS14MenuTopButton";
    public const string ProfileControl = "DS14MenuProfileControl";
    public const string ProfileLabel = "DS14MenuProfileLabel";
    public const string ProfileSection = "DS14MenuProfileSection";
    public const string ReadyButton = "DS14MenuReadyButton";
    public const string JobPriorityPreferred = "DS14MenuJobPriorityPreferred";
    public const string JobPriorityNever = "DS14MenuJobPriorityNever";
    public const string AntagPreferenceOn = "DS14MenuAntagPreferenceOn";
    public const string AntagPreferenceOff = "DS14MenuAntagPreferenceOff";
    public const string CharacterIcon = "DS14MenuCharacterIcon";
    public const string Title = "DS14MenuTitle";
    public const string Subtitle = "DS14MenuSubtitle";
    public const string RoundStatusTitle = "DS14MenuRoundStatusTitle";
    public const string RoundStatusTime = "DS14MenuRoundStatusTime";

    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var shellTex = ResCache.GetTexture("/Textures/Interface/Nano/lobby_b.png");
        var shell = new StyleBoxTexture
        {
            Texture = shellTex,
            Mode = StyleBoxTexture.StretchMode.Tile
        };
        shell.SetPatchMargin(StyleBox.Margin.All, 24);
        shell.SetExpandMargin(StyleBox.Margin.All, -4);
        shell.SetContentMarginOverride(StyleBox.Margin.All, 10);

        var topShell = new StyleBoxTexture(shell)
        {
            Texture = shellTex,
            Mode = StyleBoxTexture.StretchMode.Tile
        };
        topShell.SetContentMarginOverride(StyleBox.Margin.Vertical, 7);
        topShell.SetContentMarginOverride(StyleBox.Margin.Horizontal, 10);

        var panel = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#1A1E25EF"),
            BorderColor = Color.FromHex("#4C5666"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 10,
            ContentMarginBottomOverride = 10,
            ContentMarginLeftOverride = 10,
            ContentMarginRightOverride = 10,
        };

        var panelDark = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#12161DF1"),
            BorderColor = Color.FromHex("#394352"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 8,
            ContentMarginBottomOverride = 8,
            ContentMarginLeftOverride = 8,
            ContentMarginRightOverride = 8,
        };

        var header = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#202631F5"),
            BorderColor = Color.FromHex("#5D6A7C"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            ContentMarginTopOverride = 8,
            ContentMarginBottomOverride = 8,
            ContentMarginLeftOverride = 10,
            ContentMarginRightOverride = 10,
        };

        var inset = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#10141BE8"),
            BorderColor = Color.FromHex("#3F4958"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 6,
            ContentMarginBottomOverride = 6,
            ContentMarginLeftOverride = 6,
            ContentMarginRightOverride = 6,
        };

        var roundStatus = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#121821F3"),
            BorderColor = Color.FromHex("#5F6C7E"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 7,
            ContentMarginBottomOverride = 7,
            ContentMarginLeftOverride = 18,
            ContentMarginRightOverride = 18,
        };

        var accent = new StyleBoxFlat
        {
            BackgroundColor = sheet.HighlightPalette.Text,
            ContentMarginLeftOverride = 2,
            ContentMarginBottomOverride = 2,
        };

        var accentDim = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#566171"),
            ContentMarginLeftOverride = 1,
            ContentMarginBottomOverride = 1,
        };

        var characterIcon = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#070B10F5"),
            BorderColor = Color.FromHex("#263842"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 3,
            ContentMarginBottomOverride = 3,
            ContentMarginLeftOverride = 3,
            ContentMarginRightOverride = 3,
        };

        var tabsPanel = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#12161DEA"),
            BorderColor = Color.FromHex("#4C5666"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 10,
            ContentMarginBottomOverride = 10,
            ContentMarginLeftOverride = 10,
            ContentMarginRightOverride = 10,
        };

        var tabActive = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#48536D"),
            BorderColor = Color.FromHex("#68758E"),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 7,
            ContentMarginRightOverride = 7,
            ContentMarginTopOverride = 3,
            ContentMarginBottomOverride = 3,
        };

        var tabInactive = new StyleBoxFlat(tabActive)
        {
            BackgroundColor = Color.FromHex("#1D2330"),
            BorderColor = Color.FromHex("#374252"),
        };

        var topButton = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#111923F0"),
            BorderColor = Color.FromHex("#1D5B73"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 6,
            ContentMarginBottomOverride = 6,
            ContentMarginLeftOverride = 14,
            ContentMarginRightOverride = 14,
        };

        var topButtonHover = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#16263AF5"),
            BorderColor = Color.FromHex("#1D8BAD"),
        };

        var topButtonPressed = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#183751F5"),
            BorderColor = Color.FromHex("#2EA7D0"),
        };

        var topButtonDisabled = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#10161FC8"),
            BorderColor = Color.FromHex("#293844"),
        };

        var profileControl = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#111923F0"),
            BorderColor = Color.FromHex("#2D4757"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
            ContentMarginLeftOverride = 9,
            ContentMarginRightOverride = 9,
        };

        var profileControlHover = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#162638F4"),
            BorderColor = Color.FromHex("#1D7E9D"),
        };

        var profileControlPressed = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#18344AF5"),
            BorderColor = Color.FromHex("#2EA7D0"),
        };

        var optionDropdownBackground = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#0D1219F6"),
            BorderColor = Color.FromHex("#2D4757"),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 2,
            ContentMarginBottomOverride = 2,
            ContentMarginLeftOverride = 2,
            ContentMarginRightOverride = 2,
        };

        var profileControlDisabled = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#10161FC8"),
            BorderColor = Color.FromHex("#293844"),
        };

        var profileControlNegative = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#2E171CF2"),
            BorderColor = Color.FromHex("#9D3F49"),
        };

        var profileControlNegativeHover = new StyleBoxFlat(profileControlNegative)
        {
            BackgroundColor = Color.FromHex("#431E25F6"),
            BorderColor = Color.FromHex("#F85149"),
        };

        var topButtonNegative = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#2E171CF2"),
            BorderColor = Color.FromHex("#9D3F49"),
        };

        var topButtonNegativeHover = new StyleBoxFlat(topButtonNegative)
        {
            BackgroundColor = Color.FromHex("#431E25F6"),
            BorderColor = Color.FromHex("#F85149"),
        };

        var actionButton = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#111A25F2"),
            BorderColor = Color.FromHex("#1D5B73"),
            ContentMarginTopOverride = 8,
            ContentMarginBottomOverride = 8,
            ContentMarginLeftOverride = 14,
            ContentMarginRightOverride = 14,
        };

        var actionButtonHover = new StyleBoxFlat(actionButton)
        {
            BackgroundColor = Color.FromHex("#162A40F6"),
            BorderColor = Color.FromHex("#1D8BAD"),
        };

        var actionButtonPressed = new StyleBoxFlat(actionButton)
        {
            BackgroundColor = Color.FromHex("#193A54F6"),
            BorderColor = Color.FromHex("#2EA7D0"),
        };

        var actionButtonDisabled = new StyleBoxFlat(actionButton)
        {
            BackgroundColor = Color.FromHex("#101720C8"),
            BorderColor = Color.FromHex("#293844"),
        };

        var readyNotReady = new StyleBoxFlat(actionButton)
        {
            BackgroundColor = Color.FromHex("#572329F4"),
            BorderColor = Color.FromHex("#C44B55"),
        };

        var readyNotReadyHover = new StyleBoxFlat(readyNotReady)
        {
            BackgroundColor = Color.FromHex("#6B2931F8"),
            BorderColor = Color.FromHex("#F85149"),
        };

        var readyReady = new StyleBoxFlat(actionButton)
        {
            BackgroundColor = Color.FromHex("#1D5635F4"),
            BorderColor = Color.FromHex("#2EA043"),
        };

        var jobPriorityPreferred = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#1D5635F3"),
            BorderColor = Color.FromHex("#2EA043"),
        };

        var jobPriorityNever = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#572329F3"),
            BorderColor = Color.FromHex("#F85149"),
        };

        var antagPreferenceOn = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#1D5635F3"),
            BorderColor = Color.FromHex("#2EA043"),
        };

        var antagPreferenceOff = new StyleBoxFlat(topButton)
        {
            BackgroundColor = Color.FromHex("#572329F3"),
            BorderColor = Color.FromHex("#F85149"),
        };

        var profilePriorityPreferred = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#1D5635F3"),
            BorderColor = Color.FromHex("#2EA043"),
        };

        var profilePriorityNever = new StyleBoxFlat(profileControl)
        {
            BackgroundColor = Color.FromHex("#572329F3"),
            BorderColor = Color.FromHex("#F85149"),
        };

        return
        [
            E<PanelContainer>().Class(Shell).Panel(shell),
            E<PanelContainer>().Class(TopShell).Panel(topShell),
            E<PanelContainer>().Class(Panel).Panel(panel),
            E<PanelContainer>().Class(PanelDark).Panel(panelDark),
            E<PanelContainer>().Class(Header).Panel(header),
            E<PanelContainer>().Class(Inset).Panel(inset),
            E<PanelContainer>().Class(RoundStatus).Panel(roundStatus),
            E<PanelContainer>().Class(Accent).Panel(accent),
            E<PanelContainer>().Class(AccentDim).Panel(accentDim),
            E<PanelContainer>().Class(CharacterIcon).Panel(characterIcon),
            E<PanelContainer>().Class(OptionButton.StyleClassOptionsBackground).Panel(optionDropdownBackground),
            E<TabContainer>()
                .Class(Tabs)
                .Prop(TabContainer.StylePropertyPanelStyleBox, tabsPanel)
                .Prop(TabContainer.StylePropertyTabStyleBox, tabActive)
                .Prop(TabContainer.StylePropertyTabStyleBoxInactive, tabInactive)
                .Prop(TabContainer.stylePropertyTabFontColor, Color.FromHex("#F1F3F6"))
                .Prop(TabContainer.StylePropertyTabFontColorInactive, Color.FromHex("#AEB6C2"))
                .Prop("font", sheet.BaseFont.GetFont(12)),

            E<Label>()
                .Class(Title)
                .Font(sheet.BaseFont.GetFont(16, FontKind.Bold))
                .FontColor(sheet.HighlightPalette.Text),
            E<Label>()
                .Class(Subtitle)
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(Color.FromHex("#9BA6AD")),
            E<Label>()
                .Class(ProfileLabel)
                .Font(sheet.BaseFont.GetFont(12))
                .FontColor(Color.FromHex("#F1F3F6")),
            E<Label>()
                .Class(ProfileSection)
                .Font(sheet.BaseFont.GetFont(12))
                .FontColor(sheet.HighlightPalette.Text),
            E<Label>()
                .Class(RoundStatusTitle)
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold))
                .FontColor(sheet.HighlightPalette.Text),
            E<Label>()
                .Class(RoundStatusTime)
                .Font(sheet.BaseFont.GetFont(16, FontKind.Bold))
                .FontColor(Color.FromHex("#F1F3F6")),

            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .PseudoNormal()
                .Box(actionButton)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .PseudoHovered()
                .Box(actionButtonHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .PseudoPressed()
                .Box(actionButtonPressed)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .PseudoDisabled()
                .Box(actionButtonDisabled)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .MinHeight(42),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ActionButton)
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),

            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .PseudoNormal()
                .Box(topButton)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .PseudoHovered()
                .Box(topButtonHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .PseudoPressed()
                .Box(topButtonPressed)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .PseudoDisabled()
                .Box(topButtonDisabled)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(StyleClass.Negative)
                .PseudoNormal()
                .Box(topButtonNegative)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(StyleClass.Negative)
                .PseudoHovered()
                .Box(topButtonNegativeHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(JobPriorityPreferred)
                .PseudoPressed()
                .Box(jobPriorityPreferred)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(JobPriorityNever)
                .PseudoPressed()
                .Box(jobPriorityNever)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(AntagPreferenceOn)
                .PseudoPressed()
                .Box(antagPreferenceOn)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .Class(AntagPreferenceOff)
                .PseudoPressed()
                .Box(antagPreferenceOff)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .MinHeight(32),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(TopButton)
                .ParentOf(E<Label>().Class(OptionButton.StyleClassOptionButton))
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),

            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .PseudoNormal()
                .Box(profileControl)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .PseudoHovered()
                .Box(profileControlHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .PseudoPressed()
                .Box(profileControlPressed)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .PseudoDisabled()
                .Box(profileControlDisabled)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(StyleClass.Negative)
                .PseudoNormal()
                .Box(profileControlNegative)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(StyleClass.Negative)
                .PseudoHovered()
                .Box(profileControlNegativeHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(JobPriorityPreferred)
                .PseudoPressed()
                .Box(profilePriorityPreferred)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(JobPriorityNever)
                .PseudoPressed()
                .Box(profilePriorityNever)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(AntagPreferenceOn)
                .PseudoPressed()
                .Box(profilePriorityPreferred)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .Class(AntagPreferenceOff)
                .PseudoPressed()
                .Box(profilePriorityNever)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .MinHeight(28),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(12)),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Font(sheet.BaseFont.GetFont(12)),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ProfileControl)
                .ParentOf(E<Label>().Class(OptionButton.StyleClassOptionButton))
                .Font(sheet.BaseFont.GetFont(12)),

            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ReadyButton)
                .PseudoNormal()
                .Box(readyNotReady)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ReadyButton)
                .PseudoHovered()
                .Box(readyNotReadyHover)
                .Modulate(Color.White),
            E<ContainerButton>()
                .Class(ContainerButton.StyleClassButton)
                .Class(ReadyButton)
                .PseudoPressed()
                .Box(readyReady)
                .Modulate(Color.White),
        ];
    }
}
