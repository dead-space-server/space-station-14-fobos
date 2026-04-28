using Content.Client.Eui;
using Content.Shared.DeadSpace.LawBoardConfigurator;
using Content.Shared.Eui;
using Robust.Shared.IoC;

namespace Content.Client.DeadSpace.LawBoardConfigurator;

public sealed class LawBoardConfiguratorEui : BaseEui
{
    private readonly EntityManager _entityManager;

    private readonly LawBoardConfiguratorWindow _window;
    private EntityUid _target = EntityUid.Invalid;
    private bool _serverClosing;
    private bool _hasState;
    private bool _hasBoard;

    public LawBoardConfiguratorEui()
    {
        _entityManager = IoCManager.Resolve<EntityManager>();

        _window = new LawBoardConfiguratorWindow();
        _window.OnClose += () =>
        {
            if (_serverClosing)
                return;

            SendMessage(new CloseEuiMessage());
        };
        _window.OnEjectPressed += () =>
        {
            if (_serverClosing || !_hasState || !_hasBoard)
                return;

            SendMessage(new LawBoardConfiguratorEjectMessage());
        };
        _window.OnSavePressed += () =>
        {
            if (_serverClosing || !_hasState || !_hasBoard)
                return;

            SendMessage(new LawBoardConfiguratorSaveMessage(
                _window.GetLaws(),
                _entityManager.GetNetEntity(_target),
                _window.GetBoardName()));
        };
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not LawBoardConfiguratorEuiState lawsState)
            return;

        _hasState = true;
        _hasBoard = lawsState.HasBoard;
        _target = _hasBoard ? _entityManager.GetEntity(lawsState.Target) : EntityUid.Invalid;
        _window.SetBoardPresent(_hasBoard);
        _window.SetBoardName(lawsState.BoardName);
        _window.SetLaws(lawsState.Laws);
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _serverClosing = true;
        _window.Close();
    }
}
