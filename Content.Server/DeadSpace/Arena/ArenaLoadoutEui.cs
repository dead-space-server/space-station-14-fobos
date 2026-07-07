using Content.Server.EUI;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Eui;
using Robust.Shared.Player;

namespace Content.Server.DeadSpace.Arena;

public sealed class ArenaLoadoutEui : BaseEui
{
    private readonly ArenaRuleSystem _arenaSystem;
    private readonly IEntityManager _entManager;
    private readonly EntityUid _ruleEntity;
    private readonly ICommonSession _session;

    public ArenaLoadoutEui(ArenaRuleSystem arenaSystem, EntityUid ruleEntity, ICommonSession session)
    {
        _arenaSystem = arenaSystem;
        _entManager = IoCManager.Resolve<IEntityManager>();
        _ruleEntity = ruleEntity;
        _session = session;
    }

    public override EuiStateBase GetNewState()
    {
        if (!_entManager.TryGetComponent<ArenaRuleComponent>(_ruleEntity, out var rule))
            return new ArenaLoadoutEuiState(new List<ArenaLoadoutOption>());

        return _arenaSystem.GetLoadoutState(rule);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        switch (msg)
        {
            case ArenaLoadoutSelectedMessage selected:
                _arenaSystem.SpawnPlayer(_session, _ruleEntity, selected.WeaponIndex);
                Close();
                break;
        }
    }

    public override void Opened()
    {
        StateDirty();
    }
}
