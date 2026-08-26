using Content.Client.DeadSpace.Stylesheets;
using Content.Client.PDA;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Sheetlets;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.PDA;

[CommonSheetlet]
public sealed class PdaSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        IPanelConfig panelCfg = sheet;

        // DS14-start
        var contentBackground = new StyleBoxFlat
        {
            BackgroundColor = DeadSpaceStylePalette.SurfaceInset,
            BorderColor = DeadSpaceStylePalette.BorderInset,
            BorderThickness = new Thickness(1),
        };

        var accentBackground = StyleBoxHelpers.SquareStyleBox(sheet);
        var shellBackground = StyleBoxHelpers.BaseStyleBox(sheet);
        var borderRect = sheet.GetTexture(panelCfg.GeometricPanelBorderPath).IntoPatch(StyleBox.Margin.All, 10);
        borderRect.Modulate = DeadSpaceStylePalette.CyanBright.WithAlpha(217f / 255f);

        var settingsNormal = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Control,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            9,
            4);
        var settingsHover = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlHover,
        };
        var settingsPressed = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlPressed,
        };
        var settingsDisabled = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        var settingsPositive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            DeadSpaceStylePalette.PositiveBorder,
            new Thickness(1),
            9,
            4);
        var settingsPositiveHover = new StyleBoxFlat(settingsPositive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositiveHover,
            BorderColor = DeadSpaceStylePalette.PositiveBorderHover,
        };
        var settingsPositivePressed = new StyleBoxFlat(settingsPositive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositivePressed,
            BorderColor = DeadSpaceStylePalette.PositiveBorderPressed,
        };

        var programNormal = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.ListItem,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            6,
            4);
        var programHover = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemHover,
        };
        var programPressed = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemPressed,
        };
        var programDisabled = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        var homeRow = DeadSpaceStyleBoxes.Flat(
            Color.Transparent,
            horizontalMargin: 6,
            verticalMargin: 3);
        var homeRowHover = new StyleBoxFlat(homeRow)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlHover,
        };
        var homeRowPressed = new StyleBoxFlat(homeRow)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlPressed,
        };
        var homeRowDisabled = new StyleBoxFlat(homeRow)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
        };
        // DS14-end

        return
        [
            //PDA - Backgrounds
            E<PanelContainer>()
                .Class("PdaContentBackground")
                // DS14-start
                .Prop(PanelContainer.StylePropertyPanel, contentBackground)
                .Prop(Control.StylePropertyModulateSelf, Color.White),
                // DS14-end

            E<PanelContainer>()
                .Class("PdaBackground")
                // DS14-start
                .Prop(PanelContainer.StylePropertyPanel, accentBackground)
                .Prop(Control.StylePropertyModulateSelf, DeadSpaceStylePalette.SurfaceIcon),
                // DS14-end

            E<PanelContainer>()
                .Class("PdaBackgroundRect")
                // DS14-start
                .Prop(PanelContainer.StylePropertyPanel, shellBackground)
                .Prop(Control.StylePropertyModulateSelf, DeadSpaceStylePalette.SurfaceStatus),
                // DS14-end

            E<PanelContainer>()
                .Class("PdaBorderRect")
                .Prop(PanelContainer.StylePropertyPanel, borderRect), // DS14

            //PDA - Buttons
            // DS14-start
            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Box(settingsNormal),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Box(settingsHover),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Box(settingsPressed),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .Box(settingsDisabled),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Box(settingsPositive),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Box(settingsPositiveHover),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Box(settingsPositivePressed),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.Text),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.Text),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.Text),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.TextPlaceholder),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.PositiveBorderPressed),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.PositiveBorderPressed),

            E<PdaSettingsButton>()
                .Class(DeadSpaceStyleClass.ControlPositive)
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .ParentOf(E())
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.PositiveBorderPressed),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Box(programNormal),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Box(programHover),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Box(programPressed),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .Box(programDisabled),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassNormal))
                .Box(homeRow),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassHover))
                .Box(homeRowHover),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassPressed))
                .Box(homeRowPressed),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassDisabled))
                .Box(homeRowDisabled),
            // DS14-end

            //PDA - Text
            E<Label>()
                .Class("PdaContentFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.TextMuted), // DS14

            E<Label>()
                .Class("PdaWindowFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, DeadSpaceStylePalette.TextMuted), // DS14
        ];
    }
}
