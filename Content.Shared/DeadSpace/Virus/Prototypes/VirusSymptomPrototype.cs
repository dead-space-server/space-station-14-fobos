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
    ///     Цена мутации.
    /// </summary>
    [DataField]
    public int Price = 100;

    /// <summary>
    ///     Тип симптома.
    /// </summary>
    [DataField(required: true)]
    public VirusSymptom SymptomType;

    /// <summary>
    ///     Индикатор, требуется для управления сиптомами в случайных вирусах событий игры.
    /// </summary>
    [DataField("danger", required: true)]
    public DangerIndicatorSymptom DangerIndicator;
}

public enum DangerIndicatorSymptom
{
    Low = 0,
    Medium,
    High,
    Cataclysm
}

