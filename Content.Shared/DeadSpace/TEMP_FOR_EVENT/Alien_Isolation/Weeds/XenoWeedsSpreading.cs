using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeadSpace.TEMP_FOR_EVENT.Alien_Isolation.Weeds;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WeedSpreadingComponent : Component
{
    // Задержка перед следующим шагом распространения
    [DataField]
    public TimeSpan SpreadDelay = TimeSpan.FromSeconds(4);

    // Точное время, когда должен произойти следующий шаг (вычисляется в коде)
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan SpreadAt;
}
