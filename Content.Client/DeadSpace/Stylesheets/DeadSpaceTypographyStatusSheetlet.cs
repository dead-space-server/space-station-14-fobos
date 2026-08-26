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
        // Semantic color must never change a button's measured size. Keep the ordinary
        // semantic controls on the same 14x2 content margins as the global Button baseline.
        var positive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            DeadSpaceStylePalette.PositiveBorder,
            new Thickness(1),
            14,
            2);
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
        var positiveDisabled = new StyleBoxFlat(positive)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        var negative = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Negative,
            DeadSpaceStylePalette.NegativeBorder,
            new Thickness(1),
            14,
            2);
        var negativeHover = new StyleBoxFlat(negative)
        {
            BackgroundColor = DeadSpaceStylePalette.NegativeHover,
            BorderColor = DeadSpaceStylePalette.NegativeBorderHover,
        };
        var negativeDisabled = new StyleBoxFlat(negative)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        var warning = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.WarningControl,
            DeadSpaceStylePalette.WarningBorder,
            new Thickness(1),
            14,
            2);
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
        var warningDisabled = new StyleBoxFlat(warning)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };

        // Large lobby actions retain their deliberate 14x8 geometry in every semantic state.
        var actionPositive = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            DeadSpaceStylePalette.PositiveBorder,
            new Thickness(1),
            14,
            8);
        var actionPositiveHover = new StyleBoxFlat(actionPositive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositiveHover,
            BorderColor = DeadSpaceStylePalette.PositiveBorderHover,
        };
        var actionPositivePressed = new StyleBoxFlat(actionPositive)
        {
            BackgroundColor = DeadSpaceStylePalette.PositivePressed,
            BorderColor = DeadSpaceStylePalette.PositiveBorderPressed,
        };
        var topActionNegative = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Negative,
            DeadSpaceStylePalette.NegativeBorder,
            new Thickness(1),
            14,
            6);
        var topActionNegativeHover = new StyleBoxFlat(topActionNegative)
        {
            BackgroundColor = DeadSpaceStylePalette.NegativeHover,
            BorderColor = DeadSpaceStylePalette.NegativeBorderHover,
        };
        var topActionNegativeDisabled = new StyleBoxFlat(topActionNegative)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
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
        var priorityNever = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.NegativeStrong,
            DeadSpaceStylePalette.NegativeBorderStrong,
            new Thickness(1),
            14,
            2);
        var listUnread = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Positive,
            DeadSpaceStylePalette.PositiveBorder,
            new Thickness(1),
            6,
            4);
        var listUnreadHover = new StyleBoxFlat(listUnread)
        {
            BackgroundColor = DeadSpaceStylePalette.PositiveHover,
            BorderColor = DeadSpaceStylePalette.PositiveBorderHover,
        };
        var listUnreadDisabled = new StyleBoxFlat(listUnread)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
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
            Button(DeadSpaceStyleClass.ActionPositive).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.Ready).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E()).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.TopAction).ParentOf(E<Label>().Class(OptionButton.StyleClassOptionButton)).Font(sheet.BaseFont.GetFont(12, FontKind.Bold)),
            Button(DeadSpaceStyleClass.ControlDanger).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),
            Button(DeadSpaceStyleClass.ListItem).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),
            Button(DeadSpaceStyleClass.ListItemAlternate).ParentOf(E<Label>()).Font(sheet.BaseFont.GetFont(12)),

            // ActionPositive is only assigned to Action buttons. Match the semantic class directly so it
            // remains authoritative even when the engine resolves equal-specificity class combinations.
            Button(DeadSpaceStyleClass.ActionPositive).PseudoNormal().Box(actionPositive).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ActionPositive).PseudoHovered().Box(actionPositiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ActionPositive).PseudoPressed().Box(actionPositivePressed).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoNormal().Box(topActionNegative).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoHovered().Box(topActionNegativeHover).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoPressed().Box(topActionNegativeHover).Modulate(Color.White),
            CompoundButton(DeadSpaceStyleClass.TopAction, StyleClass.Negative).PseudoDisabled().Box(topActionNegativeDisabled).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoNormal().Box(positive).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoPressed().Box(positivePressed).Modulate(Color.White),
            Button(StyleClass.Positive).PseudoDisabled().Box(positiveDisabled).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoNormal().Box(negative).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoHovered().Box(negativeHover).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoPressed().Box(negativeHover).Modulate(Color.White),
            Button(StyleClass.Negative).PseudoDisabled().Box(negativeDisabled).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoNormal().Box(positive).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoHovered().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoPressed().Box(positivePressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlPositive).PseudoDisabled().Box(positiveDisabled).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoNormal().Box(warning).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoHovered().Box(warningHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoPressed().Box(warningPressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlWarning).PseudoDisabled().Box(warningDisabled).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoNormal().Box(negative).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoHovered().Box(negativeHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoPressed().Box(negativeHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ControlDanger).PseudoDisabled().Box(negativeDisabled).Modulate(Color.White),

            Button(DeadSpaceStyleClass.Ready).PseudoNormal().Box(readyOff).Modulate(Color.White),
            Button(DeadSpaceStyleClass.Ready).PseudoHovered().Box(readyOffHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.Ready).PseudoPressed().Box(actionPositiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.JobPriorityPreferred).PseudoPressed().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.JobPriorityNever).PseudoPressed().Box(priorityNever).Modulate(Color.White),
            Button(DeadSpaceStyleClass.AntagPreferenceOn).PseudoPressed().Box(positiveHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.AntagPreferenceOff).PseudoPressed().Box(priorityNever).Modulate(Color.White),

            Button(DeadSpaceStyleClass.ListItemUnread).PseudoNormal().Box(listUnread).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).PseudoHovered().Box(listUnreadHover).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).PseudoPressed().Box(listPressed).Modulate(Color.White),
            Button(DeadSpaceStyleClass.ListItemUnread).PseudoDisabled().Box(listUnreadDisabled).Modulate(Color.White),
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
