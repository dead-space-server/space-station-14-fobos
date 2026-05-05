using Content.Server.EUI;
using Content.Server.Popups;
using Content.Server.Silicons.Laws;
using Content.Shared.DeadSpace.LawBoardConfigurator;
using Content.Shared.Eui;
using Content.Shared.Lock;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using System.Linq;

namespace Content.Server.DeadSpace.LawBoardConfigurator;

public sealed class LawBoardConfiguratorEui : BaseEui
{
    private readonly SiliconLawSystem _siliconLawSystem;
    private readonly EntityManager _entityManager;
    private readonly ItemSlotsSystem _itemSlots;
    private readonly PopupSystem _popup;
    private readonly MetaDataSystem _metaData;
    private readonly Action<LawBoardConfiguratorEui>? _onClosed;

    private EntityUid _console;
    private string _boardSlot;
    private EntityUid _board = EntityUid.Invalid;
    private bool _hasBoard;
    private string _boardName = string.Empty;
    private List<SiliconLaw> _laws = new();

    public LawBoardConfiguratorEui(
        SiliconLawSystem siliconLawSystem,
        EntityManager entityManager,
        ItemSlotsSystem itemSlots,
        PopupSystem popup,
        MetaDataSystem metaData,
        EntityUid console,
        string boardSlot,
        Action<LawBoardConfiguratorEui>? onClosed = null)
    {
        _siliconLawSystem = siliconLawSystem;
        _entityManager = entityManager;
        _itemSlots = itemSlots;
        _popup = popup;
        _metaData = metaData;
        _console = console;
        _boardSlot = boardSlot;
        _onClosed = onClosed;
    }

    public override EuiStateBase GetNewState()
    {
        var board = _hasBoard ? _entityManager.GetNetEntity(_board) : NetEntity.Invalid;
        return new LawBoardConfiguratorEuiState(_laws, board, _boardName, _hasBoard);
    }

    public void RefreshFromConsole()
    {
        _hasBoard = false;
        _board = EntityUid.Invalid;
        _boardName = string.Empty;
        _laws.Clear();

        if (!_itemSlots.TryGetSlot(_console, _boardSlot, out var slot) || slot.Item is not { } board)
        {
            StateDirty();
            return;
        }

        if (!_entityManager.HasComponent<SiliconLawProviderComponent>(board))
        {
            StateDirty();
            return;
        }

        var ev = new GetSiliconLawsEvent(board);
        _entityManager.EventBus.RaiseLocalEvent(board, ref ev);
        if (!ev.Handled)
        {
            StateDirty();
            return;
        }

        _hasBoard = true;
        _board = board;
        _boardName = _entityManager.TryGetComponent<MetaDataComponent>(_board, out var meta)
            ? meta.EntityName
            : string.Empty;
        _laws = ev.Laws.Laws.Select(x => x.ShallowClone()).ToList();
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!TryGetAttachedEntity(Player, out var attached))
            return;

        if (_entityManager.TryGetComponent<LockComponent>(_console, out var lockComp) && lockComp.Locked)
        {
            _popup.PopupEntity(Loc.GetString("law-board-configurator-locked"), _console, attached);
            Close();
            return;
        }

        if (msg is LawBoardConfiguratorEjectMessage)
        {
            if (!_hasBoard)
                return;

            EjectBoard(attached);
            return;
        }

        if (msg is not LawBoardConfiguratorSaveMessage message)
            return;

        if (!_hasBoard)
        {
            RefreshFromConsole();
            return;
        }

        if (!_itemSlots.TryGetSlot(_console, _boardSlot, out var slot) || slot.Item != _board)
        {
            RefreshFromConsole();
            return;
        }

        if (_entityManager.GetEntity(message.Target) != _board)
            return;

        if (!_entityManager.HasComponent<SiliconLawProviderComponent>(_board))
            return;

        var ev = new GetSiliconLawsEvent(_board);
        _entityManager.EventBus.RaiseLocalEvent(_board, ref ev);
        if (!ev.Handled)
            return;

        var newLaws = message.Laws.Select(x =>
        {
            var clone = x.ShallowClone();
            if (clone.LawString.Length > LawBoardConfiguratorLimits.LawTextMaxLength)
                clone.LawString = clone.LawString[..LawBoardConfiguratorLimits.LawTextMaxLength];
            return clone;
        }).Take(LawBoardConfiguratorLimits.LawCountMax).ToList();
        _siliconLawSystem.SetLaws(newLaws, _board);
        _laws = newLaws.Select(x => x.ShallowClone()).ToList();

        var newBoardName = message.BoardName.Trim();
        if (newBoardName.Length > LawBoardConfiguratorLimits.BoardNameMaxLength)
            newBoardName = newBoardName[..LawBoardConfiguratorLimits.BoardNameMaxLength];

        if (!string.IsNullOrWhiteSpace(newBoardName) && newBoardName != _boardName)
        {
            _metaData.SetEntityName(_board, newBoardName);
            _boardName = newBoardName;
        }

        _popup.PopupEntity(Loc.GetString("law-board-configurator-saved"), attached, attached);
        StateDirty();
    }

    private void EjectBoard(EntityUid user)
    {
        if (!_hasBoard)
            return;

        if (!_itemSlots.TryGetSlot(_console, _boardSlot, out var slot) || slot.Item != _board)
        {
            RefreshFromConsole();
            return;
        }

        if (!_itemSlots.TryEjectToHands(_console, slot, user))
            return;

        RefreshFromConsole();
    }

    private static bool TryGetAttachedEntity(ICommonSession session, out EntityUid entity)
    {
        if (session.AttachedEntity is { } attached)
        {
            entity = attached;
            return true;
        }

        entity = default;
        return false;
    }

    public override void Closed()
    {
        base.Closed();
        _onClosed?.Invoke(this);
    }
}
