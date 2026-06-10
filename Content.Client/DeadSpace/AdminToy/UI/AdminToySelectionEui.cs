// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Eui;
using Content.Shared.DeadSpace.AdminToy;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client.DeadSpace.AdminToy.UI;

[UsedImplicitly]
public sealed class AdminToySelectionEui : BaseEui
{
    private readonly IEntityManager _entityManager;
    private readonly AdminToySelectionMenu _window;

    public AdminToySelectionEui()
    {
        _entityManager = IoCManager.Resolve<IEntityManager>();
        _window = new AdminToySelectionMenu();
        _window.OnClose += OnClosed;
        _window.ToySelected += (toy, name, description, ttsVoice) =>
            SendMessage(new AdminToySelectedMessage(toy, name, description, ttsVoice));
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        var toyState = (AdminToySelectionEuiState) state;
        var target = _entityManager.GetEntity(toyState.Target);

        if (_entityManager.TryGetComponent<MetaDataComponent>(target, out var meta))
            _window.SetTargetName(meta.EntityName);
    }

    private void OnClosed()
    {
        SendMessage(new CloseEuiMessage());
    }
}
