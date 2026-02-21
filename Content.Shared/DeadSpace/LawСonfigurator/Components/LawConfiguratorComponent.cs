using Content.Shared.DeadSpace.LawConfigurator.Systems;
using Content.Shared.DoAfter;
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
    [DataField]
    public SoundSpecifier? SuccessSound;

    /// <summary>
    /// Задержка (прогресс бар)
    /// </summary>
    [DataField("doAfter")]
    public float DoAfter = 10.0f;

    /// <summary>
    /// Требуется ли открытая панель для настройки законов юнита
    /// </summary>
    [DataField("requireOpenPanel")]
    public bool RequireOpenPanel = true;

    /// <summary>
    /// Есть ли плата в слоте конфигуратора законов
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HasBoard;

    /// <summary>
    /// Состояние спрайта, когда плата отсутствует
    /// </summary>
    [DataField, AutoNetworkedField]
    public string EmptyState = "icon";

    /// <summary>
    /// Состояние спрайта, когда плата вставлена
    /// </summary>
    [DataField, AutoNetworkedField]
    public string FilledState = "icon-filled";
}
