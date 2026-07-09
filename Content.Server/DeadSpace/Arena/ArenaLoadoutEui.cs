using Content.Server.EUI;
using Content.Shared.DeadSpace.Arena;
using Content.Shared.Eui;
using Robust.Shared.Player;

namespace Content.Server.DeadSpace.Arena;

public sealed class ArenaLoadoutEui : BaseEui
{
    private readonly ArenaSystem _arena;
    private readonly ICommonSession _session;

    public ArenaLoadoutEui(ArenaSystem arena, ICommonSession session)
    {
        _arena = arena;
        _session = session;
    }

    public override EuiStateBase GetNewState()
    {
        return _arena.GetLoadoutState();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        if (msg is ArenaLoadoutSelectedMessage selected)
        {
            _arena.SpawnPlayer(_session, selected.WeaponIndex);
            Close();
        }
    }

    public override void Opened()
    {
        StateDirty();
    }
}
