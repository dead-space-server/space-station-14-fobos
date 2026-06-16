// DS14
// DS14-Start
// Система наложения спрайта боевого режима для боргов
//    Добавляет визуальный оверлей (подсветку) на спрайт борга
//    при активации боевого режима. Используется GenericVisualizer
//    для переключения видимости слоя combat_overlay.
// DS14-End
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Borgs;

[RegisterComponent, NetworkedComponent]
public sealed partial class BorgCombatModeOverlayComponent : Component
{
}

[Serializable, NetSerializable]
public enum BorgCombatModeVisuals : byte
{
    Combat,
}
