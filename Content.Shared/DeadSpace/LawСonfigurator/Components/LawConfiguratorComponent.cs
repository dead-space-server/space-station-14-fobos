using Content.Shared.DeadSpace.LawConfigurator.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.LawConfigurator.Components;

[Access(typeof(LawConfiguratorSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class LawConfiguratorComponent : Component
{
    /// <summary>
    /// Звук при успешной настройке
    /// </summary>
    [DataField("successSound")]
    public SoundSpecifier? SuccessSound;

    /// <summary>
    /// Есть ли плата в слоте
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HasBoard;
}