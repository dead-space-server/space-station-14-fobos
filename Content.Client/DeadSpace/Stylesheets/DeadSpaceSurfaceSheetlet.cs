// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.DeadSpace.Stylesheets;

[CommonSheetlet]
public sealed class DeadSpaceSurfaceSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        var shellTexture = ResCache.GetTexture("/Textures/Interface/Nano/lobby_b.png");
        var shell = new StyleBoxTexture
        {
            Texture = shellTexture,
            Mode = StyleBoxTexture.StretchMode.Tile,
        };
        shell.SetPatchMargin(StyleBox.Margin.All, 24);
        shell.SetExpandMargin(StyleBox.Margin.All, -4);
        shell.SetContentMarginOverride(StyleBox.Margin.All, 10);

        var topShell = new StyleBoxTexture(shell)
        {
            Texture = shellTexture,
            Mode = StyleBoxTexture.StretchMode.Tile,
        };
        topShell.SetContentMarginOverride(StyleBox.Margin.Vertical, 7);
        topShell.SetContentMarginOverride(StyleBox.Margin.Horizontal, 10);

        var panel = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Surface,
            DeadSpaceStylePalette.Border,
            new Thickness(1),
            10,
            10);
        var panelDark = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceDark,
            DeadSpaceStylePalette.BorderDark,
            new Thickness(1),
            8,
            8);
        var panelWarning = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Warning,
            DeadSpaceStylePalette.Amber,
            new Thickness(1));
        var modalScrim = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ModalScrim);
        var flatBody = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.SurfaceFlat, horizontalMargin: 8);
        flatBody.ContentMarginTopOverride = 8;
        flatBody.ContentMarginBottomOverride = 6;

        var header = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceHeader,
            DeadSpaceStylePalette.BorderHeader,
            new Thickness(0, 0, 0, 1),
            10,
            8);
        var inset = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceInset,
            DeadSpaceStylePalette.BorderInset,
            new Thickness(1),
            6,
            6);
        var roundStatus = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceStatus,
            DeadSpaceStylePalette.BorderHeader,
            new Thickness(1),
            18,
            7);

        var accent = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.Amber);
        accent.ContentMarginLeftOverride = 2;
        accent.ContentMarginBottomOverride = 2;
        var accentDim = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.AccentDim);
        accentDim.ContentMarginLeftOverride = 1;
        accentDim.ContentMarginBottomOverride = 1;
        var lowDivider = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.AccentDim);

        var characterIcon = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceIcon,
            DeadSpaceStylePalette.BorderIcon,
            new Thickness(1),
            3,
            3);
        var defaultTabsPanel = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceTabs,
            DeadSpaceStylePalette.Border,
            new Thickness(1));
        var defaultTabActive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceTabActive,
            DeadSpaceStylePalette.BorderTabActive,
            new Thickness(1),
            5);
        var defaultTabInactive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceTabInactive,
            DeadSpaceStylePalette.BorderTabInactive,
            new Thickness(1),
            5);

        var listHeader = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceHeader,
            DeadSpaceStylePalette.BorderHeader,
            new Thickness(0, 0, 0, 1),
            7,
            5);
        var listItem = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.ListItem,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            6,
            4);
        var listItemAlternate = new StyleBoxFlat(listItem)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemAlternate,
        };
        var input = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Input,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            7,
            4);
        var popup = new StyleBoxFlat(panel)
        {
            BackgroundColor = DeadSpaceStylePalette.SurfacePopup,
            BorderColor = DeadSpaceStylePalette.BorderHeader,
        };
        var optionBackground = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Input,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1));
        // Preserve the zero-margin geometry of widespread legacy panel classes while changing only their chrome.
        var legacyPanelDeep = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.SurfaceDark);
        var legacyPanelInsetDeep = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceInset,
            DeadSpaceStylePalette.BorderInset,
            new Thickness(2));
        var defaultWindowPanel = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Surface,
            DeadSpaceStylePalette.Border,
            new Thickness(1));
        var defaultWindowHeader = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.SurfaceHeader,
            DeadSpaceStylePalette.BorderHeader,
            new Thickness(0, 0, 0, 1));

        return
        [
            E<PanelContainer>().Class(DeadSpaceStyleClass.Window).Panel(shell),
            E<PanelContainer>().Class(DeadSpaceStyleClass.WindowTop).Panel(topShell),
            E<PanelContainer>().Class(DeadSpaceStyleClass.Surface).Panel(panel),
            E<PanelContainer>().Class(DeadSpaceStyleClass.SurfaceDark).Panel(panelDark),
            E<PanelContainer>().Class(DeadSpaceStyleClass.SurfaceWarning).Panel(panelWarning),
            E<PanelContainer>().Class(DeadSpaceStyleClass.ModalScrim).Panel(modalScrim),
            E<PanelContainer>().Class(DeadSpaceStyleClass.SurfaceFlat).Panel(flatBody),
            E<PanelContainer>().Class(DeadSpaceStyleClass.SectionHeader).Panel(header),
            E<PanelContainer>().Class(DeadSpaceStyleClass.Inset).Panel(inset),
            E<PanelContainer>().Class(DeadSpaceStyleClass.RoundStatus).Panel(roundStatus),
            E<PanelContainer>().Class(DeadSpaceStyleClass.Accent).Panel(accent),
            E<PanelContainer>().Class(DeadSpaceStyleClass.AccentDim).Panel(accentDim),
            // The legacy low divider appears across content UI; keep it quiet and one logical pixel thick.
            E<PanelContainer>()
                .Class(StyleClass.LowDivider)
                .Panel(lowDivider)
                .MinSize(new Vector2(1, 1)),
            E<PanelContainer>().Class(DeadSpaceStyleClass.CharacterIcon).Panel(characterIcon),
            E<PanelContainer>().Class(DeadSpaceStyleClass.ListHeader).Panel(listHeader),
            E<PanelContainer>().Class(DeadSpaceStyleClass.ListItem).Panel(listItem),
            E<PanelContainer>().Class(DeadSpaceStyleClass.ListItemAlternate).Panel(listItemAlternate),
            E<PanelContainer>().Class(DeadSpaceStyleClass.TextField).Panel(input),
            E<PanelContainer>().Class(DeadSpaceStyleClass.Popup).Panel(popup),
            E<PanelContainer>().Class(OptionButton.StyleClassOptionsBackground).Panel(optionBackground),
            E<PanelContainer>().Class(StyleClass.PanelDeep).Panel(legacyPanelDeep),
            E<PanelContainer>().Class(StyleClass.PanelInsetDeep).Panel(legacyPanelInsetDeep),
            E<PanelContainer>().Class("BackgroundDark").Panel(legacyPanelDeep),
            // Standard content windows use DS chrome without per-window style annotations.
            E<PanelContainer>().Class(DefaultWindow.StyleClassWindowPanel).Panel(defaultWindowPanel),
            E<PanelContainer>().Class(DefaultWindow.StyleClassWindowHeader).Panel(defaultWindowHeader),
            E<PanelContainer>().Class("BackgroundPanel").Panel(defaultWindowPanel),
            E<PanelContainer>().Class("WindowHeadingBackground").Panel(defaultWindowHeader),
            E<TabContainer>()
                .Prop(TabContainer.StylePropertyPanelStyleBox, defaultTabsPanel)
                .Prop(TabContainer.StylePropertyTabStyleBox, defaultTabActive)
                .Prop(TabContainer.StylePropertyTabStyleBoxInactive, defaultTabInactive)
                .Prop(TabContainer.stylePropertyTabFontColor, DeadSpaceStylePalette.Text)
                .Prop(TabContainer.StylePropertyTabFontColorInactive, DeadSpaceStylePalette.TextInactive)
                .Prop("font", sheet.BaseFont.GetFont(12)),
        ];
    }
}
