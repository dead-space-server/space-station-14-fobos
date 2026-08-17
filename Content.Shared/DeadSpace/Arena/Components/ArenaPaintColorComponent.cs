using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared.DeadSpace.Arena;

/// <summary>
/// Отмечает игрока или предмет снаряжения в режиме Пеинтболл: клиент полностью
/// окрашивает объект в цвет команды, сервер использует цвет для пятен краски на полу.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class ArenaPaintColorComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [DataField, AutoNetworkedField]
    public ArenaTeam Team = ArenaTeam.None;
}