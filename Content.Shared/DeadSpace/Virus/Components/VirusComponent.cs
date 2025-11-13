// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.Whitelist;
using Content.Shared.Humanoid.Prototypes;
using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Virus.Components;

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
    public float Threshold = 100f;

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

    /// <summary>
    ///     Допустимые к заражению сущности.
    /// </summary>
    [DataField]
    public EntityWhitelist? EntityWhitelist = new();

    /// <summary>
    ///     Допустимые к заражению расы.
    /// </summary>
    [DataField]
    public List<ProtoId<SpeciesPrototype>> SpeciesWhitelist = new();
}


/// <summary>
///     Класс содержит данные об вирусе.
/// </summary>
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class VirusData : ReagentData
{
    [DataField]
    public string StrainId = string.Empty;

    [DataField]
    public List<IVirusSymptom> ActiveSymptomInstances = new();

    [DataField]
    public float ComplexityVaccine = 0;

    [DataField]
    public float Threshold = 0f;

    [DataField]
    public float DefaultMedicineResistance = 0f;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float> MedicineResistance = new();

    [DataField]
    public float Infectivity = 0f;

    [DataField]
    public List<ProtoId<SpeciesPrototype>> SpeciesWhitelist = new();

    public override bool Equals(ReagentData? other)
    {
        if (other is not VirusData o)
            return false;

        if (StrainId != o.StrainId)
            return false;

        if (!MathHelper.CloseTo(ComplexityVaccine, o.ComplexityVaccine))
            return false;

        if (!MathHelper.CloseTo(Threshold, o.Threshold))
            return false;

        if (!MathHelper.CloseTo(DefaultMedicineResistance, o.DefaultMedicineResistance))
            return false;

        if (!MathHelper.CloseTo(Infectivity, o.Infectivity))
            return false;

        if (!SpeciesWhitelist.SequenceEqual(o.SpeciesWhitelist))
            return false;

        if (MedicineResistance.Count != o.MedicineResistance.Count ||
            MedicineResistance.Except(o.MedicineResistance).Any())
            return false;

        // Проверяем симптомы по типу, а не по объекту (чтобы не упираться в разные экземпляры)
        if (ActiveSymptomInstances.Count != o.ActiveSymptomInstances.Count)
            return false;

        for (var i = 0; i < ActiveSymptomInstances.Count; i++)
        {
            if (ActiveSymptomInstances[i].Type != o.ActiveSymptomInstances[i].Type)
                return false;
        }

        return true;
    }

    public override ReagentData Clone()
    {
        return new VirusData
        {
            StrainId = StrainId,
            ComplexityVaccine = ComplexityVaccine,
            Threshold = Threshold,
            DefaultMedicineResistance = DefaultMedicineResistance,
            Infectivity = Infectivity,

            // Глубокое копирование коллекций
            ActiveSymptomInstances = ActiveSymptomInstances
                .Select(s => s.Clone())
                .ToList(),

            MedicineResistance = MedicineResistance
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

            SpeciesWhitelist = new List<ProtoId<SpeciesPrototype>>(SpeciesWhitelist)
        };
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(StrainId);
        hash.Add(ComplexityVaccine);
        hash.Add(Threshold);
        hash.Add(DefaultMedicineResistance);
        hash.Add(Infectivity);

        foreach (var kvp in MedicineResistance)
            hash.Add(kvp.Key);
        foreach (var s in SpeciesWhitelist)
            hash.Add(s);

        // Симптомы учитываем по типам
        foreach (var symptom in ActiveSymptomInstances)
            hash.Add(symptom.Type);

        return hash.ToHashCode();
    }

}