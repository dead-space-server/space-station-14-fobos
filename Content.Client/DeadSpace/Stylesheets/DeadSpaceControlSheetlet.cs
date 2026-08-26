// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Chat.Controls;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.DeadSpace.Stylesheets;

[CommonSheetlet]
public sealed class DeadSpaceControlSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        var action = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Action,
            DeadSpaceStylePalette.CyanDim,
            new Thickness(1),
            14,
            8);
        var actionHover = new StyleBoxFlat(action)
        {
            BackgroundColor = DeadSpaceStylePalette.ActionHover,
            BorderColor = DeadSpaceStylePalette.Cyan,
        };
        var actionPressed = new StyleBoxFlat(action)
        {
            BackgroundColor = DeadSpaceStylePalette.ActionPressed,
            BorderColor = DeadSpaceStylePalette.CyanBright,
        };
        var actionDisabled = new StyleBoxFlat(action)
        {
            BackgroundColor = DeadSpaceStylePalette.ActionDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };

        var topAction = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Control,
            DeadSpaceStylePalette.CyanDim,
            new Thickness(1),
            14,
            6);
        var topActionHover = new StyleBoxFlat(topAction)
        {
            BackgroundColor = DeadSpaceStylePalette.ActionHover,
            BorderColor = DeadSpaceStylePalette.Cyan,
        };
        var topActionPressed = new StyleBoxFlat(topAction)
        {
            BackgroundColor = DeadSpaceStylePalette.ActionPressed,
            BorderColor = DeadSpaceStylePalette.CyanBright,
        };
        var topActionDisabled = new StyleBoxFlat(topAction)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };

        var dangerControl = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Control,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            14,
            2);
        var dangerControlHover = new StyleBoxFlat(dangerControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlHover,
            BorderColor = DeadSpaceStylePalette.Cyan,
        };
        var dangerControlPressed = new StyleBoxFlat(dangerControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlPressed,
            BorderColor = DeadSpaceStylePalette.CyanBright,
        };
        var dangerControlDisabled = new StyleBoxFlat(dangerControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        // Match the existing base button content margins so a global chrome change cannot resize layouts.
        var baseControl = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Control,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            14,
            2);
        var baseControlHover = new StyleBoxFlat(baseControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlHover,
            BorderColor = DeadSpaceStylePalette.Cyan,
        };
        var baseControlPressed = new StyleBoxFlat(baseControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlPressed,
            BorderColor = DeadSpaceStylePalette.CyanBright,
        };
        var baseControlDisabled = new StyleBoxFlat(baseControl)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        // Bare ContainerButton is widely used as a zero-margin interactive row. Give it safe global chrome
        // without changing its measured geometry; the more specific ordinary Button rule below keeps its margins.
        var bareContainer = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.Control);
        var bareContainerHover = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ControlHover);
        var bareContainerPressed = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ControlPressed);
        var bareContainerDisabled = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ControlDisabled);

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
        var listItemHover = new StyleBoxFlat(listItem)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemHover,
            BorderColor = DeadSpaceStylePalette.Cyan,
        };
        var listItemPressed = new StyleBoxFlat(listItem)
        {
            BackgroundColor = DeadSpaceStylePalette.ListItemPressed,
            BorderColor = DeadSpaceStylePalette.CyanBright,
        };
        var listItemDisabled = new StyleBoxFlat(listItem)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        // ListContainerButton historically had a zero-margin StyleBoxOverride. Preserve that geometry.
        var listContainer = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ListItem);
        var listContainerHover = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ListItemHover);
        var listContainerPressed = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ListItemPressed);
        var listContainerDisabled = DeadSpaceStyleBoxes.Flat(DeadSpaceStylePalette.ControlDisabled);

        var input = DeadSpaceStyleBoxes.Flat(
            DeadSpaceStylePalette.Input,
            DeadSpaceStylePalette.BorderControl,
            new Thickness(1),
            7,
            4);
        var inputDisabled = new StyleBoxFlat(input)
        {
            BackgroundColor = DeadSpaceStylePalette.ControlDisabled,
            BorderColor = DeadSpaceStylePalette.BorderDisabled,
        };
        var textArea = new StyleBoxFlat(input);
        textArea.ContentMarginTopOverride = 6;
        textArea.ContentMarginBottomOverride = 6;

        var rules = new List<StyleRule>
        {
            E<ItemList>()
                .Prop(ItemList.StylePropertyBackground, input)
                .Prop(ItemList.StylePropertyItemBackground, listItem)
                .Prop(ItemList.StylePropertyDisabledItemBackground, inputDisabled)
                .Prop(ItemList.StylePropertySelectedItemBackground, listItemPressed),
            E<OutputPanel>()
                .Prop(OutputPanel.StylePropertyStyleBox, textArea),
            // ChatWindowPanel owns the chat background and its configurable opacity.
            E<ChatBox>()
                .ParentOf(E<PanelContainer>().Class(ChatInputBox.StyleClassChatPanel))
                .ParentOf(E<BoxContainer>())
                .ParentOf(E<OutputPanel>())
                .Prop(OutputPanel.StylePropertyStyleBox, new StyleBoxEmpty()),
            E<LineEdit>()
                .Prop(LineEdit.StylePropertyStyleBox, input)
                .Prop("font-color", DeadSpaceStylePalette.Text),
            E<LineEdit>()
                .Class(LineEdit.StyleClassLineEditNotEditable)
                .Prop(LineEdit.StylePropertyStyleBox, inputDisabled)
                .Prop("font-color", DeadSpaceStylePalette.TextMuted),
            E<LineEdit>()
                .Pseudo(LineEdit.StylePseudoClassPlaceholder)
                .Prop("font-color", DeadSpaceStylePalette.TextPlaceholder),
            E<TextEdit>()
                .Prop("font-color", DeadSpaceStylePalette.Text)
                .Prop(TextEdit.StylePropertyCursorColor, DeadSpaceStylePalette.Amber)
                .Prop(TextEdit.StylePropertySelectionColor, DeadSpaceStylePalette.CyanSelection),
            E<TextEdit>()
                .Pseudo(TextEdit.StylePseudoClassPlaceholder)
                .Prop("font-color", DeadSpaceStylePalette.TextPlaceholder),
        };

        AddBareContainerButtonRules(
            rules,
            bareContainer,
            bareContainerHover,
            bareContainerPressed,
            bareContainerDisabled);
        AddCheckBoxRules(rules, bareContainer, bareContainerHover, bareContainerDisabled);
        // The switch itself communicates its value; do not tint the full row when it is on.
        rules.AddRange(
        [
            E<SwitchButton>().PseudoPressed().Box(bareContainer).Modulate(Color.White),
            E<SwitchButton>().PseudoDisabled().Box(bareContainerDisabled).Modulate(Color.White),
        ]);
        AddButtonRules(rules, null, baseControl, baseControlHover, baseControlPressed, baseControlDisabled);
        AddButtonRules(rules, DeadSpaceStyleClass.Action, action, actionHover, actionPressed, actionDisabled);
        AddButtonRules(rules, DeadSpaceStyleClass.TopAction, topAction, topActionHover, topActionPressed, topActionDisabled);
        AddButtonRules(
            rules,
            DeadSpaceStyleClass.ControlDanger,
            dangerControl,
            dangerControlHover,
            dangerControlPressed,
            dangerControlDisabled);
        AddButtonRules(rules, DeadSpaceStyleClass.ListItem, listItem, listItemHover, listItemPressed, listItemDisabled);
        AddButtonRules(rules, DeadSpaceStyleClass.ListItemAlternate, listItemAlternate, listItemHover, listItemPressed, listItemDisabled);
        AddButtonRules(rules,
            ListContainer.StyleClassListContainerButton,
            listContainer,
            listContainerHover,
            listContainerPressed,
            listContainerDisabled);

        return rules.ToArray();
    }

    private static void AddButtonRules(
        List<StyleRule> rules,
        string? styleClass,
        StyleBox normal,
        StyleBox hovered,
        StyleBox pressed,
        StyleBox disabled)
    {
        rules.AddRange(
        [
            Button(styleClass).PseudoNormal().Box(normal).Modulate(Color.White),
            Button(styleClass).PseudoHovered().Box(hovered).Modulate(Color.White),
            Button(styleClass).PseudoPressed().Box(pressed).Modulate(Color.White),
            Button(styleClass).PseudoDisabled().Box(disabled).Modulate(Color.White),
        ]);
    }

    private static MutableSelectorElement Button(string? styleClass)
    {
        var selector = E<ContainerButton>();
        return styleClass == null
            ? selector.Class(ContainerButton.StyleClassButton)
            : selector.Class(styleClass);
    }

    private static void AddBareContainerButtonRules(
        List<StyleRule> rules,
        StyleBox normal,
        StyleBox hovered,
        StyleBox pressed,
        StyleBox disabled)
    {
        rules.AddRange(
        [
            E<ContainerButton>().PseudoNormal().Box(normal).Modulate(Color.White),
            E<ContainerButton>().PseudoHovered().Box(hovered).Modulate(Color.White),
            E<ContainerButton>().PseudoPressed().Box(pressed).Modulate(Color.White),
            E<ContainerButton>().PseudoDisabled().Box(disabled).Modulate(Color.White),
        ]);
    }

    private static void AddCheckBoxRules(
        List<StyleRule> rules,
        StyleBox normal,
        StyleBox hovered,
        StyleBox disabled)
    {
        // A checked CheckBox uses the pressed pseudo-class permanently. Keep the row chrome neutral and let the
        // checkbox texture communicate true/false, otherwise every enabled option becomes a solid cyan stripe.
        rules.AddRange(
        [
            E<CheckBox>().PseudoNormal().Box(normal).Modulate(Color.White),
            E<CheckBox>().PseudoHovered().Box(hovered).Modulate(Color.White),
            E<CheckBox>().PseudoPressed().Box(normal).Modulate(Color.White),
            E<CheckBox>().PseudoDisabled().Box(disabled).Modulate(Color.White),
        ]);
    }
}
