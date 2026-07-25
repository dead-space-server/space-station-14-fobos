using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaLoadoutEuiState : EuiStateBase
{
    public List<ArenaLoadoutOption> Weapons { get; }

    public ArenaLoadoutEuiState(List<ArenaLoadoutOption> weapons)
    {
        Weapons = weapons;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaLoadoutOption
{
    public int Index { get; set; }
    public LocId Name { get; set; } = string.Empty;
    public LocId Description { get; set; } = string.Empty;
    public LocId Category { get; set; } = string.Empty;
    public string SpritePrototype { get; set; } = string.Empty;
}

[Serializable, NetSerializable]
public sealed class ArenaLoadoutSelectedMessage : EuiMessageBase
{
    public int WeaponIndex { get; }

    public ArenaLoadoutSelectedMessage(int weaponIndex)
    {
        WeaponIndex = weaponIndex;
    }
}
