// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Client.DeadSpace.Stylesheets;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.DeadSpace.Traitor;

public sealed class TraitorUltraOfferWindow : DefaultWindow
{
    private const float ContentWidth = 620f;

    public readonly RichTextLabel BodyLabel;
    public readonly RichTextLabel GainsLabel;
    public readonly RichTextLabel LossesLabel;
    public readonly Button AcceptButton;
    public readonly Button DeclineButton;

    public TraitorUltraOfferWindow()
    {
        MinSize = new Vector2(620, 400);
        SetSize = new Vector2(700, 470);
        HeaderClass = DeadSpaceMenuSheetlet.Header;
        TitleClass = DeadSpaceMenuSheetlet.Title;

        BodyLabel = MakeTextLabel();
        GainsLabel = MakeTextLabel();
        LossesLabel = MakeTextLabel();

        AcceptButton = new Button
        {
            HorizontalExpand = true,
            MinHeight = 34,
            TextAlign = Label.AlignMode.Center,
            StyleClasses = { DeadSpaceMenuSheetlet.ProfileControl, DeadSpaceMenuSheetlet.ProfileControlPositive },
        };
        DeclineButton = new Button
        {
            HorizontalExpand = true,
            MinHeight = 34,
            TextAlign = Label.AlignMode.Center,
            StyleClasses = { DeadSpaceMenuSheetlet.ProfileControl, StyleClass.Negative },
        };

        var shell = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            StyleClasses = { DeadSpaceMenuSheetlet.Shell },
        };

        var panel = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            StyleClasses = { DeadSpaceMenuSheetlet.PanelDark },
        };

        var textContent = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            SeparationOverride = 10,
            HorizontalExpand = true,
            MaxWidth = ContentWidth + 32,
            Children =
            {
                new PanelContainer
                {
                    HorizontalExpand = true,
                    StyleClasses = { DeadSpaceMenuSheetlet.Inset },
                    Children = { BodyLabel },
                },
                new PanelContainer
                {
                    HorizontalExpand = true,
                    StyleClasses = { DeadSpaceMenuSheetlet.ListRow },
                    Children = { GainsLabel },
                },
                new PanelContainer
                {
                    HorizontalExpand = true,
                    StyleClasses = { DeadSpaceMenuSheetlet.ListRowAlt },
                    Children = { LossesLabel },
                },
            }
        };

        panel.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            SeparationOverride = 10,
            Children =
            {
                new ScrollContainer
                {
                    HorizontalExpand = true,
                    VerticalExpand = true,
                    HScrollEnabled = false,
                    VScrollEnabled = true,
                    Children = { textContent },
                },
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Align = AlignMode.Center,
                    SeparationOverride = 8,
                    Margin = new Thickness(2, 0),
                    Children =
                    {
                        AcceptButton,
                        DeclineButton,
                    }
                }
            }
        });

        shell.AddChild(panel);
        Contents.AddChild(shell);
    }

    private static RichTextLabel MakeTextLabel()
    {
        return new RichTextLabel
        {
            HorizontalExpand = true,
            MaxWidth = ContentWidth,
            Margin = new Thickness(10, 7),
        };
    }

    public void SetState(string title, string body, string gains, string losses, string accept, string decline)
    {
        Title = title;
        BodyLabel.SetMessage(body);
        GainsLabel.SetMessage(gains);
        LossesLabel.SetMessage(losses);
        AcceptButton.Text = accept;
        DeclineButton.Text = decline;
    }
}
