// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Virus.Prototypes;

[Prototype("virusSymptom")]
public sealed partial class VirusSymptomPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    [DataField]
    public string Description { get; private set; } = default!;

    /// <summary>
    ///     Количество прибавляемой заразности симптому в процентах.
    /// </summary>
    [DataField]
    public float AddInfectivity { get; private set; } = 0.02f;

    /// <summary>
    ///     Цена мутации.
    /// </summary>
    [DataField]
    public int Price = 100;

    /// <summary>
    ///     Тип симптома. Должен быть уникальным.
    ///     DEPRECATED: Use SymptomTypeClass instead. This field will be removed in a future version
    ///     after all prototypes have been migrated to use SymptomTypeClass.
    /// </summary>
    [DataField]
    public VirusSymptom? SymptomType;

    /// <summary>
    ///     Fully qualified type name of the IVirusSymptom implementation class.
    ///     Example: "Content.Server.DeadSpace.Virus.Symptoms.CoughSymptom"
    ///     This is the preferred method for specifying symptom types.
    /// </summary>
    [DataField]
    public string? SymptomTypeClass;

    /// <summary>
    ///     Индикатор, требуется для управления сиптомами в случайных вирусах событий игры.
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

