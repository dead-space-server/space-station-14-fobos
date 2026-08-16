using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Arena;

/// <summary>
/// Отмечает предметы снаряжения (броня, шлем), которые в режиме TDM должны окрашиваться в цвет команды.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class ArenaTeamClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public ArenaTeam Team = ArenaTeam.None;
}
