using Content.Client.DeadSpace.Stylesheets;
using Content.Client.PDA;
using Content.Client.Stylesheets;
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
        // DS14-start
        var contentBackground = new StyleBoxFlat
        {
            BackgroundColor = DeadSpaceStylePalette.SurfaceInset,
        };

        // These panels are tinted at runtime from PdaBorderColorComponent. Keep their source color white so the
        // prototype color is preserved instead of being multiplied by an already-dark DS14 surface.
        var accentBackground = DeadSpaceStyleBoxes.Flat(Color.White);
        var shellBackground = DeadSpaceStyleBoxes.Flat(Color.White);
        var borderRect = DeadSpaceStyleBoxes.Flat(
            Color.Transparent,
            DeadSpaceStylePalette.BorderDark,
            new Thickness(1));

        var settingsNormal = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Control,
            Color.Transparent,
            new Thickness(1),
            9,
            4);
        var settingsHover = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlHover,
            BorderColor = DeadSpaceStylePalette.HoverOutline,
        };
        var settingsPressed = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlPressed,
            BorderColor = DeadSpaceStylePalette.PressedOutline,
        };
        var settingsDisabled = new StyleBoxFlat(settingsNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = Color.Transparent,
        };
        var settingsPositive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            Color.Transparent,
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
            Color.Transparent,
            new Thickness(1),
            6,
            4);
        var programHover = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemHover,
            BorderColor = DeadSpaceStylePalette.HoverOutline,
        };
        var programPressed = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemPressed,
            BorderColor = DeadSpaceStylePalette.PressedOutline,
        };
        var programDisabled = new StyleBoxFlat(programNormal)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = Color.Transparent,
        };
        var homeRow = DeadSpaceStyleBoxes.Flat(
            Color.Transparent,
            horizontalMargin: 6,
            verticalMargin: 3);
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
                .Box(homeRow),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassPressed))
                .Box(homeRow),

            E<BoxContainer>()
                .Class("PdaHomeSummary")
                .ParentOf(E<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassDisabled))
                .Box(homeRow),
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
