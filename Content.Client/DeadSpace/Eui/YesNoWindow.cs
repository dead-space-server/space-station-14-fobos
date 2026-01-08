// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Eui
{
    public sealed class YesNoWindow : DefaultWindow
    {
        public readonly Button NoButton;
        public readonly Button YesButton;

        public Label MessageLabel { get; }

        public YesNoWindow(string title, string text)
        {
            Title = title;

            MessageLabel = new Label { Text = text };

            Contents.AddChild(new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Children =
                {
                    new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Children =
                        {
                            MessageLabel,
                            new BoxContainer
                            {
                                Orientation = LayoutOrientation.Horizontal,
                                Align = AlignMode.Center,
                                Children =
                                {
                                    (YesButton = new Button { Text = "Yes" }),
                                    new Control { MinSize = new Vector2(20, 0) },
                                    (NoButton = new Button { Text = "No" })
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
