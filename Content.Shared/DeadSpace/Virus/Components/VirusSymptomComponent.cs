// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Virus.Components;

/// <summary>
///     Base component for virus symptoms. Contains metadata about the symptom.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class VirusSymptomComponent : Component
{
    /// <summary>
    ///     Name of the symptom.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    /// <summary>
    ///     Description of the symptom.
    /// </summary>
    [DataField]
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    ///     Количество прибавляемой заразности симптому в процентах.
    /// </summary>
    [DataField]
    public float AddInfectivity { get; private set; } = 0.02f;

    /// <summary>
    ///     Цена мутации.
    /// </summary>
    [DataField]
    public int Price { get; private set; } = 100;

    /// <summary>
    ///     Индикатор опасности симптома.
    /// </summary>
    [DataField("danger", required: true)]
    public DangerIndicatorSymptom DangerIndicator;

    /// <summary>
    ///     Минимальный интервал срабатывания симптома
    /// </summary>
    [DataField]
    public float MinInterval = 15f;

    /// <summary>
    ///     Максимальный интервал срабатывания симптома
    /// </summary>
    [DataField]
    public float MaxInterval = 60f;
}

public enum DangerIndicatorSymptom
{
    Low = 0,
    Medium,
    High,
    Cataclysm
}
