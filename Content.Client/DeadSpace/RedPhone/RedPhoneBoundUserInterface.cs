// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.RedPhone;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.RedPhone;

[UsedImplicitly]
public sealed class RedPhoneBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private RedPhoneWindow? _window;

    public RedPhoneBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RedPhoneWindow>();
        _window.Title = Loc.GetString("red-phone-window-title", ("title", EntMan.GetComponent<MetaDataComponent>(Owner).EntityName));
        _window.StartCall += target => SendMessage(new RedPhoneStartCallMessage(target));
        _window.EndCall += () => SendMessage(new RedPhoneEndCallMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is RedPhoneBoundUiState castState)
            _window?.UpdateState(castState);
    }
}
