using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

// DS14
[CommonSheetlet]
public sealed class LobbyBackgroundSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var backgroundTex = ResCache.GetTexture("/Textures/Interface/Nano/lobby_b.png");
        var background = new StyleBoxTexture
        {
            Texture = backgroundTex,
            Mode = StyleBoxTexture.StretchMode.Tile
        };

        background.SetPatchMargin(StyleBox.Margin.All, 24);
        background.SetExpandMargin(StyleBox.Margin.All, -4);
        background.SetContentMarginOverride(StyleBox.Margin.All, 8);

        return
        [
            E<PanelContainer>()
                .Class("LobbyBackground")
                .Prop(PanelContainer.StylePropertyPanel, background),
        ];
    }
}
