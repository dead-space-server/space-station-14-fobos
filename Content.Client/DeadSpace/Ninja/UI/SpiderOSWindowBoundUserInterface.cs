// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.Ninja.UI;

public sealed class SpiderOSWindowBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SpiderOSWindow? _menu;

    public SpiderOSWindowBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SpiderOSWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
    }
}
