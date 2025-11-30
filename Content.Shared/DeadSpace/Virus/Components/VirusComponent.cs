// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.Whitelist;
using Content.Shared.Humanoid.Prototypes;
using System.Linq;
using Robust.Shared.Serialization;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Body.Prototypes;

namespace Content.Shared.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusComponent : Component
{
    /// <summary>
    ///     Данные об вирусе.
    /// </summary>
    [DataField]
    public VirusData Data = new();

    /// <summary>
    ///     Список активных симптомов для этого инфицированного тела.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public List<IVirusSymptom> ActiveSymptomInstances = new();

    /// <summary>
    ///     Окно времени обновления вируса.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow VirusUpdateWindow = default!;

    public VirusComponent(VirusData data)
    {
        Data = data;
    }
}


/// <summary>
///     Класс содержит данные об вирусе.
/// </summary>
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class VirusData : ReagentData
{
    /// <summary>
    ///     ID штамма.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string StrainId = string.Empty;

    /// <summary>
    ///     Список симптомов которые должны быть при инициализации.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<VirusSymptomPrototype>> ActiveSymptom = new();

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
    public List<ProtoId<BodyPrototype>> BodyWhitelist = new();

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

        if (!BodyWhitelist.SequenceEqual(o.BodyWhitelist))
            return false;

        if (MedicineResistance.Count != o.MedicineResistance.Count ||
            MedicineResistance.Except(o.MedicineResistance).Any())
            return false;

        if (!ActiveSymptom.SequenceEqual(o.ActiveSymptom))
            return false;

        if (EntityWhitelist is null && o.EntityWhitelist is null)
            return true;

        if (EntityWhitelist is null || o.EntityWhitelist is null)
            return false;

        return EntityWhitelist.Equals(o.EntityWhitelist);
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

            ActiveSymptom = ActiveSymptom,

            MedicineResistance = MedicineResistance
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

            BodyWhitelist = BodyWhitelist,

            EntityWhitelist = EntityWhitelist is null
                ? null
                : new EntityWhitelist
                {
                    Components = EntityWhitelist.Components?.ToArray(),
                    Sizes = EntityWhitelist.Sizes?.ToList(),
                    Tags = EntityWhitelist.Tags?.ToList(),
                    RequireAll = EntityWhitelist.RequireAll
                }
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
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }

        foreach (var s in BodyWhitelist)
            hash.Add(s);

        foreach (var symptom in ActiveSymptom)
            hash.Add(symptom);

        if (EntityWhitelist != null)
        {
            if (EntityWhitelist.Components != null)
                foreach (var c in EntityWhitelist.Components)
                    hash.Add(c);

            if (EntityWhitelist.Sizes != null)
                foreach (var s in EntityWhitelist.Sizes)
                    hash.Add(s);

            if (EntityWhitelist.Tags != null)
                foreach (var t in EntityWhitelist.Tags)
                    hash.Add(t);

            hash.Add(EntityWhitelist.RequireAll);
        }

        return hash.ToHashCode();
    }

}