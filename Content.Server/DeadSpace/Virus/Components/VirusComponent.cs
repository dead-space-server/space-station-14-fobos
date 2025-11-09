// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Server.DeadSpace.Virus.Symptoms;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusComponent : Component
{
    /// <summary>
    ///     ID штамма.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string StrainId = string.Empty;

    /// <summary>
    ///     Список активных симптомов для этого инфицированного тела.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public List<IVirusSymptom> ActiveSymptomInstances = new();

    /// <summary>
    ///     Сложность разработки вакцины.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float ComplexityVaccine = 0;

    /// <summary>
    ///     Живучесть вируса. Если <= 0.1, организм считается вылеченным.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float Threshold = 100;

    /// <summary>
    ///     Стандартное значение сопротивления медикаментам (антибиотикам).
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float DefaultMedicineResistance = 0f;

    /// <summary>
    ///     Сопротивление медикаментам, модификатор урона.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<ProtoId<ReagentPrototype>, float> MedicineResistance = new();

    /// <summary>
    ///     Показатель заразности вируса от 0 до 1.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float Infectivity = 0.1f;
}
