// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.DeadSpace.Stylesheets;

[CommonSheetlet]
public sealed class DeadSpaceTypographyStatusSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        var positive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            DeadSpaceStylePalette.PositiveBorder,
            new Thickness(1),
            9,
            4);
        var positiveHover = new StyleBoxFlat(positive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositiveHover,
            BorderColor = DeadSpaceStylePalette.PositiveBorderHover,
        };
        var positivePressed = new StyleBoxFlat(positive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositivePressed,
            BorderColor = DeadSpaceStylePalette.PositiveBorderPressed,
        };
        var negative = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Negative,
            DeadSpaceStylePalette.NegativeBorder,
            new Thickness(1),
            9,
            4);
        var negativeHover = new StyleBoxFlat(negative)
        {
            BackgroundColor = DeadSpaceStylePalette.NegativeHover,
            BorderColor = DeadSpaceStylePalette.NegativeBorderHover,
        };
        var warning = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.WarningControl,
            DeadSpaceStylePalette.WarningBorder,
            new Thickness(1),
            9,
            4);
        var warningHover = new StyleBoxFlat(warning)
        {
            BackgroundColor = DeadSpaceStylePalette.WarningControlHover,
            BorderColor = DeadSpaceStylePalette.WarningBorderHover,
        };
        var warningPressed = new StyleBoxFlat(warning)
        {
            BackgroundColor = DeadSpaceStylePalette.WarningControlPressed,
            BorderColor = DeadSpaceStylePalette.WarningBorderHover,
        };
        var readyOff = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.NegativeStrong,
            DeadSpaceStylePalette.NegativeBorderStrong,
            new Thickness(1),
            14,
            8);
        var readyOffHover = new StyleBoxFlat(readyOff)
        {
            BackgroundColor = DeadSpaceStylePalette.NegativeStrongHover,
            BorderColor = DeadSpaceStylePalette.NegativeBorderHover,
        };
        var listPressed = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.ListItemPressed,
            DeadSpaceStylePalette.CyanBright,
            new Thickness(1),
            6,
            4);
        var progressHighlight = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.Amber);
        progressHighlight.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);
        var progressAccent = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.Cyan);
        progressAccent.SetContentMarginOverride(StyleBox.Margin.Vertical, 14.5f);

        var rules = new List<StyleRule>
        {
            // Ordinary content text uses the DS palette without a BodyText annotation.
            E<Label>().FontColor(DeadSpaceStylePalette.Text),
            E<Label>()
                .Class(DeadSpaceStyleClass.Title)
                .Font(sheet.BaseFont.GetFont(16, FontKind.Bold))
                .FontColor(DeadSpaceStylePalette.Amber),
            E<Label>()
                .Class(DeadSpaceStyleClass.Subtitle)
                .Font(sheet.BaseFont.GetFont(10))
                .FontColor(DeadSpaceStylePalette.TextMuted),
            E<Label>()
                .Class(DeadSpaceStyleClass.SectionTitle)
                .Font(sheet.BaseFont.GetFont(12))
                .FontColor(DeadSpaceStylePalette.Amber),
            E<Label>()
                .Class(DeadSpaceStyleClass.ListHeader)
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold))
                .FontColor(DeadSpaceStylePalette.Amber),
            E<Label>()
                .Class(DeadSpaceStyleClass.RoundStatusTitle)
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold))
                .FontColor(DeadSpaceStylePalette.Amber),
            E<Label>()
                .Class(DeadSpaceStyleClass.RoundStatusTime)
                .Font(sheet.BaseFont.GetFont(16, FontKind.Bold))
                .FontColor(DeadSpaceStylePalette.Text),
            // RichTextLabel does not expose a stylesheet color property; keep its typography aligned here.
            E<RichTextLabel>()
                .Class(DeadSpaceStyleClass.Subtitle)
                .Font(sheet.BaseFont.GetFont(10)),
            E<RichTextLabel>()
                .Class(DeadSpaceStyleClass.SectionTitle)
                .Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            E<RichTextLabel>()
                .Class(DeadSpaceStyleClass.Title)
                .Font(sheet.BaseFont.GetFont(16, FontKind.Bold)),
            E<ProgressBar>()
                .Class(DeadSpaceStyleClass.ProgressHighlight)
                .Prop(ProgressBar.StylePropertyForeground, progressHighlight),
            E<ProgressBar>()
                .Class(DeadSpaceStyleClass.ProgressAccent)
                .Prop(ProgressBar.StylePropertyForeground, progressAccent),

            Button(DeadSpaceStyleClass.Action).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E()).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E<Label>().Class(OptionButton.StyleClassOptionButton)).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.ControlDanger).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),
            Button(DeadSpaceStyleClass.ListItem).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),
            Button(DeadSpaceStyleClass.ListItemAlternate).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),

            CompoundButton(DeadSpaceStyleClass.Action, DeadSpaceStyleClass.ActionPositive).PseudoNormal().Box(positive).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.Action, DeadSpaceStyleClass.ActionPositive).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.Action, DeadSpaceStyleClass.ActionPositive).PseudoPressed().Box(positivePressed).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoNormal().Box(negative).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoHovered().Box(negativeHover).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoNormal().Box(positive).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoPressed().Box(positivePressed).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoNormal().Box(negative).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoHovered().Box(negativeHover).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoPressed().Box(negativeHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoNormal().Box(positive).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoPressed().Box(positivePressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoNormal().Box(warning).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoHovered().Box(warningHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoPressed().Box(warningPressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoNormal().Box(negative).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoHovered().Box(negativeHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoPressed().Box(negativeHover).Modulate(Color.White),

            Button(DeadSpaceStyleClass.Ready).PseudoNormal().Box(readyOff).Modulate(Color.White),
            Button(DeadSpaceStyleClass.Ready).PseudoHovered().Box(readyOffHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.Ready).PseudoPressed().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.JobPriorityPreferred).PseudoPressed().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.JobPriorityNever).PseudoPressed().Box(readyOff).Modulate(Color.White),
            Button(DeadSpaceStyleClass.AntagPreferenceOn).PseudoPressed().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.AntagPreferenceOff).PseudoPressed().Box(readyOff).Modulate(Color.White),

            Button(DeadSpaceStyleClass.ListItemUnread).PseudoNormal().Box(positive).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).PseudoPressed().Box(listPressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),
        };

        return rules.ToArray();
    }

    private static MutableSelectorElement Button(string styleClass)
    {
        return E<ContainerButton>().Class(styleClass);
    }

    private static MutableSelectorElement CompoundButton(string firstClass, string secondClass)
    {
        return Button(firstClass).Class(secondClass);
    }
}
