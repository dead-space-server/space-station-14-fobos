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
            BackgroundColor = Color.FromHex("#10141BE8"),
            BorderColor = Color.FromHex("#3F4958"),
            BorderThickness = new Thickness(1),
        };

        var accentBackground = StyleBoxHelpers.SquareStyleBox(sheet);
        var shellBackground = StyleBoxHelpers.BaseStyleBox(sheet);
        var borderRect = sheet.GetTexture(panelCfg.GeometricPanelBorderPath).IntoPatch(StyleBox.Margin.All, 10);
        borderRect.Modulate = Color.FromHex("#2EA7D0D9");
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
                .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#050A10F2")),
                // DS14-end

            E<PanelContainer>()
                .Class("PdaBackgroundRect")
                // DS14-start
                .Prop(PanelContainer.StylePropertyPanel, shellBackground)
                .Prop(Control.StylePropertyModulateSelf, Color.FromHex("#121821F3")),
                // DS14-end

            E<PanelContainer>()
                .Class("PdaBorderRect")
                .Prop(PanelContainer.StylePropertyPanel, borderRect), // DS14

            //PDA - Buttons
            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.HoverColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.PressedColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.EnabledFgColor)),

            E<PdaSettingsButton>()
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .Prop(PdaSettingsButton.StylePropertyBgColor, Color.FromHex(PdaSettingsButton.NormalBgColor))
                .Prop(PdaSettingsButton.StylePropertyFgColor, Color.FromHex(PdaSettingsButton.DisabledFgColor)),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.NormalBgColor)),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.HoverColor)),

            E<PdaProgramItem>()
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Prop(PdaProgramItem.StylePropertyBgColor, Color.FromHex(PdaProgramItem.PressedColor)), // DS14

            //PDA - Text
            E<Label>()
                .Class("PdaContentFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, Color.FromHex("#9BA6AD")), // DS14

            E<Label>()
                .Class("PdaWindowFooterText")
                .Prop(Label.StylePropertyFont, sheet.BaseFont.GetFont(10))
                .Prop(Label.StylePropertyFontColor, Color.FromHex("#9BA6AD")), // DS14
        ];
    }
}

