using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Silicons.Laws.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LawConfiguratorComponent : Component
{
    /// <summary>
    /// Время, необходимое для конфигурации законов.
    /// </summary>
    [DataField]
    public TimeSpan ConfigurationTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Звук, который воспроизводится при успешной конфигурации.
    /// </summary>
    [DataField]
    public SoundSpecifier? SuccessSound;

    /// <summary>
    /// Таймер конфигурации. Null если процесс не идет.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? ConfigurationEndTime;

    /// <summary>
    /// Цель, на которую нацелен конфигуратор.
    /// </summary>
    [DataField]
    public EntityUid? Target;

    /// <summary>
    /// Пользователь, который активировал конфигуратор.
    /// </summary>
    [DataField]
    public EntityUid? User;
}
