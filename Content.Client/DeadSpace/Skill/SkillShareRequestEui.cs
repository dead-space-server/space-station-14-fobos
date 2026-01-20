// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.Skills.Events;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using System.Numerics;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.DeadSpace.Skill;

[UsedImplicitly]
public sealed class SkillShareRequestEui : BaseEui
{
    private readonly SkillShareRequestWindow _window;

    public SkillShareRequestEui()
    {
        _window = new SkillShareRequestWindow();

        _window.YesButton.OnPressed += _ =>
        {
            SendMessage(new SkillShareResponseMessage(true));
            _window.Close();
        };

        _window.NoButton.OnPressed += _ =>
        {
            SendMessage(new SkillShareResponseMessage(false));
            _window.Close();
        };

        _window.OnClose += () => SendMessage(new SkillShareResponseMessage(false));
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is SkillShareRequestEuiState s)
        {
            _window.SetRequesterName(s.RequesterName);
        }
    }

    public override void Closed()
    {
        _window.Close();
    }
}

public sealed class SkillShareRequestWindow : DefaultWindow
{
    public readonly Button YesButton;
    public readonly Button NoButton;
    public readonly Label MessageLabel;

    public SkillShareRequestWindow()
    {
        Title = Loc.GetString("skill-share-request-title");

        MessageLabel = new Label
        {
            Text = Loc.GetString("skill-share-request-message", ("name", "")),
            HorizontalAlignment = HAlignment.Center
        };

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
                        new Control { MinSize = new Vector2(0, 20) },
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (YesButton = new Button { Text = Loc.GetString("skill-share-yes") }),
                                new Control { MinSize = new Vector2(20, 0) },
                                (NoButton = new Button { Text = Loc.GetString("skill-share-no") })
                            }
                        }
                    }
                }
            }
        });
    }

    public void SetRequesterName(string name)
    {
        MessageLabel.Text = Loc.GetString("skill-share-request-message", ("name", name));
    }
}
