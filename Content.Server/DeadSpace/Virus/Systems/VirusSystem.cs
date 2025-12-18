// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Virus;
using Content.Shared.Whitelist;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.DeadSpace.Virus.Symptoms;
using Content.Shared.Tag;
using Content.Shared.Body.Components;
using Content.Shared.Mobs;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Content.Shared.Body.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Examine;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : SharedVirusSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly VirusDiagnoserDataServerSystem _virusDiagnoserDataServer = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Метка для сущностей, которые инфецируются со 100% вероятностью.
    /// </summary>
    public readonly ProtoId<TagPrototype> VirusAlwaysInfectableTag = "VirusAlwaysInfectable";

    /// <summary>
    ///     Метка для сущностей, которые игнорируют проверку возможности заражения.
    /// </summary>
    public readonly ProtoId<TagPrototype> IgnoreCanInfectTag = "IgnoreCanInfect";
    public const SlotFlags ProtectiveSlots =
            SlotFlags.FEET |
            SlotFlags.HEAD |
            SlotFlags.EYES |
            SlotFlags.GLOVES |
            SlotFlags.MASK |
            SlotFlags.NECK |
            SlotFlags.INNERCLOTHING |
            SlotFlags.OUTERCLOTHING;
    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("VirusSystem");

        SubscribeLocalEvent<VirusComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VirusComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VirusComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<VirusComponent, CauseVirusEvent>(OnCauseVirus);
        SubscribeLocalEvent<VirusComponent, CureVirusEvent>(OnCureVirus);

        RashInitialize();
        AsphyxiaInitialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VirusComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.VirusUpdateWindow != null && component.VirusUpdateWindow.IsExpired())
            {
                component.VirusUpdateWindow.Reset();
                UpdateVirus(uid, component);
            }
        }
    }

    private void OnCauseVirus(Entity<VirusComponent> entity, ref CauseVirusEvent args)
    {
        RefreshSymptoms((entity, entity.Comp));
    }

    private void OnCureVirus(Entity<VirusComponent> entity, ref CureVirusEvent args)
    {
        // При изличении вырабатывается иммунитет
        var immun = EnsureComp<VirusImmunComponent>(entity);
        immun.StrainsId.Add(entity.Comp.Data.StrainId);
    }

    private void OnMobStateChanged(EntityUid uid, VirusComponent component, MobStateChangedEvent args)
    {
        component.PatientState = args.NewMobState;
    }

    private void UpdateVirus(EntityUid uid, VirusComponent component)
    {
        component.Data.MutationPoints += component.Data.RegenMutationPoints;

        if (CanManifestInHost((uid, component)))
        {
            foreach (var symptom in component.ActiveSymptomInstances)
            {
                symptom.OnUpdate(uid, component);
            }
        }

        if (!BaseVirusSettings.DebuffVirusMultipliers.TryGetValue(component.RegenerationType, out var regenMultiplier))
            regenMultiplier = 1.0f;

        var totalPoints = component.Data.RegenThreshold * regenMultiplier;

        if (component.PatientState is MobState.Dead)
            totalPoints = -component.Data.DamageWhenDead;

        AddThresholdPoints((uid, component), totalPoints);
    }

    private void OnComponentInit(EntityUid uid, VirusComponent component, ComponentInit args)
    {
        var whitelist = component.Data.EntityWhitelist ??= new EntityWhitelist();

        whitelist.Components ??= Array.Empty<string>();
        var compList = whitelist.Components.ToHashSet();

        compList.Add("MobState");
        compList.Add("HumanoidAppearance");
        compList.Add("Bloodstream");

        whitelist.Components = compList.ToArray();

        component.VirusUpdateWindow = new TimedWindow(1f, 1f, _timing, _random);

        RefreshSymptoms((uid, component));
    }

    private void OnShutdown(EntityUid uid, VirusComponent component, ComponentShutdown args)
    {
        foreach (var symptom in component.ActiveSymptomInstances)
        {
            symptom.OnRemoved(uid, component);
        }
    }

    /// <summary>
    ///     Изменяет здоровье вируса.
    /// </summary>
    public void AddThresholdPoints(Entity<VirusComponent?> host, float points = 1f)
    {
        if (!Resolve(host, ref host.Comp, false))
            return;

        if (host.Comp.Data.Threshold + points >= host.Comp.Data.MaxThreshold)
            return;

        host.Comp.Data.Threshold += points;

        if (host.Comp.Data.Threshold <= 0)
            CureVirus(host, host.Comp);
    }

    /// <summary>
    ///     Инфецируемый распространяет инфекцию вокруг себя.
    /// </summary>
    public void InfectAround(Entity<VirusComponent?> host, float range = 1f)
    {
        if (!Resolve(host, ref host.Comp, false))
            return;

        InfectAround(host, range, host.Comp);
    }

    /// <summary>
    ///     Добавляет интерфейсы в компонент из симптомов VirusData.
    /// </summary>
    public void RefreshSymptoms(Entity<VirusComponent?> host)
    {
        if (!Resolve(host, ref host.Comp, false))
            return;

        if (host.Comp.Data.ActiveSymptom == null || host.Comp.Data.ActiveSymptom.Count <= 0)
            return;

        foreach (var protoSymptom in host.Comp.Data.ActiveSymptom)
        {
            if (!_prototype.TryIndex(protoSymptom, out var symptom))
                continue;

            var symptomInstance = CreateSymptomInstance(symptom.SymptomType);

            // Проверяем, есть ли уже экземпляр этого типа симптома
            if (host.Comp.ActiveSymptomInstances.Any(s => s.Type == symptom.SymptomType))
                continue;

            host.Comp.ActiveSymptomInstances.Add(symptomInstance);

            symptomInstance.OnAdded(host, host.Comp);
        }
    }

    public void InfectAround(EntityUid host, float range = 1f, VirusComponent? component = null)
    {
        if (!Resolve(host, ref component, false))
            return;

        // Берём только мобов
        var entities = _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMapCoordinates(host, Transform(host)), range).ToList();

        if (entities.Count <= 0)
            return;

        foreach (var ent in entities)
        {
            var target = ent.Owner;

            if (target == host)
                continue;

            var hostPosition = _transform.GetMapCoordinates(host);
            var targetPosition = _transform.GetMapCoordinates(target);

            if (!_examine.InRangeUnOccluded(hostPosition, targetPosition, range, null))
                continue;

            ProbInfect((host, component), target);
        }
    }

    /// <summary>
    ///     Заразить с вероятностью.
    /// </summary>
    public void ProbInfect(Entity<VirusComponent?> host, EntityUid target)
    {
        if (!Resolve(host, ref host.Comp, false))
            return;

        ProbInfect(host.Comp.Data, target, host);
    }

    public void ProbInfect(VirusData data, EntityUid target, EntityUid? host = null)
    {
        var ev = new ProbInfectAttemptEvent(target, false, host);
        RaiseLocalEvent(target, ev);

        if (ev.Cancel)
            return;

        if (!CanInfect(target, data) && !_tag.HasTag(target, IgnoreCanInfectTag))
            return;

        if (_tag.HasTag(target, VirusAlwaysInfectableTag))
        {
            InfectEntity(data, target);
            return;
        }

        // Вычисляем шанс заражения
        var chance = GetVirusInfectionChance(target, data);

        // Бросаем шанс
        if (_random.Prob(chance))
        {
            _sawmill.Debug($"[{host}] заразил [{target}] вирусом {data.StrainId} (шанс {chance:P0})");
            InfectEntity(data, target);
        }
        else
        {
            _sawmill.Debug($"[{host}] не заразил [{target}] (шанс {chance:P0})");
        }
    }

    public void InfectEntity(Entity<VirusComponent?> source, EntityUid target)
    {
        if (!Resolve(source, ref source.Comp, false))
            return;

        InfectEntity(source.Comp.Data, source);
    }

    public void InfectEntity(VirusData data, EntityUid target)
    {
        var localData = (VirusData)data.Clone();

        if (TryComp<VirusComponent>(target, out var targetVirus)
            && targetVirus.Data.StrainId == localData.StrainId)
        {
            MergeMedicineResistance(localData, targetVirus.Data);
        }

        // Проверяем PrimaryPatient и другой штамм
        if (TryComp<PrimaryPacientComponent>(target, out var pacientComponent)
            && pacientComponent.StrainId != data.StrainId)
        {
            RemComp<PrimaryPacientComponent>(target);
        }

        // В любом случае копируем остальные данные (например, симптомы, тела и т.п.)
        var targetComp = EnsureComp<VirusComponent>(target);
        targetComp.Data = (VirusData)data.Clone();

        RaiseLocalEvent(target, new CauseVirusEvent(target));
    }

    private void MergeMedicineResistance(VirusData source, VirusData target)
    {
        foreach (var kvp in source.MedicineResistance)
        {
            if (source.MedicineResistance.TryGetValue(kvp.Key, out var existingValue))
            {
                // Берём лучший (максимальный) коэффициент
                target.MedicineResistance[kvp.Key] = Math.Max(existingValue, kvp.Value);
            }
            else
            {
                // Если элемента нет — добавляем
                target.MedicineResistance[kvp.Key] = kvp.Value;
            }
        }

        // Также переносим недостающие элементы из target в source, если нужно
        foreach (var kvp in target.MedicineResistance)
        {
            if (!source.MedicineResistance.ContainsKey(kvp.Key))
                source.MedicineResistance[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    ///     Возможность заразиться вирусом.
    /// </summary>
    public bool CanInfect(EntityUid target, VirusComponent component)
    {
        return CanInfect(target, component.Data);
    }

    public bool CanInfect(EntityUid target, VirusData data)
    {
        if (HasComp<ZombieComponent>(target)
            || HasComp<NecromorfComponent>(target)
            || HasComp<InfectionDeadComponent>(target)
            || HasComp<PendingZombieComponent>(target))
            return false;

        if (_mobState.IsDead(target))
            return false;

        if (TryComp<VirusImmunComponent>(target, out var immun) &&
            (immun.StrainsId.Contains(data.StrainId) || immun.ImmunAll))
        {
            return false;
        }

        if (TryComp<VirusComponent>(target, out var targetVirusComp))
        {
            // Сила вируса определяется по количеству симптомов
            if (targetVirusComp.Data.ActiveSymptom.Count >= data.ActiveSymptom.Count)
                return false;
        }

        if (!_whitelist.IsWhitelistPass(data.EntityWhitelist, target))
            return false;

        // Должно быть тело!
        if (TryComp<BodyComponent>(target, out var body)
            && body.Prototype != null
            && !data.BodyWhitelist.Contains(_prototype.Index(body.Prototype.Value)))
            return false;

        return true;
    }

    public string GenerateStrainId()
    {
        const int length = 6;

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var id = new char[length];
        for (int i = 0; i < length; i++)
        {
            id[i] = chars[_random.Next(chars.Length)];
        }

        return new string(id);
    }

    public void AddMultiPriceDeleteSymptom(string strainId, int value)
    {
        var query = EntityQueryEnumerator<VirusComponent>();
        while (query.MoveNext(out _, out var virusComponent))
        {
            if (virusComponent.Data.StrainId == strainId)
                virusComponent.Data.MultiPriceDeleteSymptom += value;
        }

        var queryServer = EntityQueryEnumerator<VirusDiagnoserDataServerComponent>();
        while (queryServer.MoveNext(out var server, out var serverComponent))
        {
            foreach (var data in serverComponent.StrainData.Values)
            {
                if (data.StrainId == strainId)
                    data.MultiPriceDeleteSymptom += value;
            }

            _virusDiagnoserDataServer.UpdateConnectedInterfaces(server, serverComponent);
        }
    }

    public VirusData GenerateVirusData(
    string strainId,
    Dictionary<DangerIndicatorSymptom, int> symptomsByDanger,
    int bodyCount)
    {
        var data = new VirusData(strainId);

        foreach (var (danger, count) in symptomsByDanger)
        {
            if (count <= 0)
                continue;

            var availableSymptoms = _prototype
                .EnumeratePrototypes<VirusSymptomPrototype>()
                .Where(p =>
                    p.DangerIndicator == danger &&
                    !data.ActiveSymptom.Contains(p.ID))
                .ToList();

            if (availableSymptoms.Count == 0)
                continue;

            var toAdd = Math.Min(count, availableSymptoms.Count);

            for (var i = 0; i < toAdd; i++)
            {
                var picked = _random.PickAndTake(availableSymptoms);
                data.ActiveSymptom.Add(picked.ID);

                if (availableSymptoms.Count == 0)
                    break;
            }
        }

        if (bodyCount > 0)
        {
            var availableBodies = _prototype
                .EnumeratePrototypes<BodyPrototype>()
                .Select(p => p.ID)
                .Where(id => !BaseVirusSettings.BodyBlackList.Contains(id) && !data.BodyWhitelist.Contains(id))
                .ToList();

            if (availableBodies.Count > 0)
            {
                var toAdd = Math.Min(bodyCount, availableBodies.Count);

                for (var i = 0; i < toAdd; i++)
                {
                    var body = _random.PickAndTake(availableBodies);
                    data.BodyWhitelist.Add(body);

                    if (availableBodies.Count == 0)
                        break;
                }
            }
        }

        return data;
    }

    public void CureVirus(EntityUid uid, VirusComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        RaiseLocalEvent(uid, new CureVirusEvent(uid));

        RemComp<VirusComponent>(uid);
    }

    /// <summary>
    ///     Пытается нанести урон вирусу антибиотиком.
    ///     С каждым применением увеличивает сопротивление к этому антибиотику.
    ///     Сопротивление никогда не опускается ниже DefaultMedicineResistance.
    /// </summary>
    public void ApplyMedicineDamage(
        Entity<VirusComponent?> target,
        ProtoId<ReagentPrototype> medicine,
        float baseDamage,
        float resistanceIncrease = 0.05f,
        float maxResistance = 0.9f)
    {
        if (!Resolve(target, ref target.Comp, false))
            return;

        var data = target.Comp.Data;

        // Получаем текущее сопротивление (не ниже дефолтного)
        if (!data.MedicineResistance.TryGetValue(medicine, out var resistance))
        {
            resistance = data.DefaultMedicineResistance;
        }
        else
        {
            resistance = Math.Max(resistance, data.DefaultMedicineResistance);
        }

        // Считаем фактический урон
        var damageMultiplier = Math.Clamp(1f - resistance, 0f, 1f);
        var finalDamage = baseDamage * damageMultiplier;

        if (finalDamage > 0f)
            AddThresholdPoints(target, -finalDamage);

        // Увеличиваем сопротивление
        var newResistance = resistance + resistanceIncrease;

        newResistance = Math.Clamp(
            newResistance,
            data.DefaultMedicineResistance,
            maxResistance
        );

        data.MedicineResistance[medicine] = newResistance;
    }

    private float GetVirusInfectionChance(EntityUid target, VirusComponent component)
    {
        return GetVirusInfectionChance(target, component.Data);
    }

    private float GetVirusInfectionChance(EntityUid target, VirusData data)
    {
        var resistanceQuery = new VirusResistanceQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(target, resistanceQuery);

        var finalChance = data.Infectivity * resistanceQuery.TotalCoefficient;

        // от 0 до 100%
        finalChance = Math.Clamp(finalChance, 0f, 1.0f);

        return finalChance;
    }

    /// <summary>
    ///     Нужно добавить новый тип вируса в этот switch.
    /// </summary>
    private IVirusSymptom CreateSymptomInstance(VirusSymptom type)
    {
        return type switch
        {
            VirusSymptom.Cough =>
                new CoughSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Vomit =>
                new VomitSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Rash =>
                new RashSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Drowsiness =>
                new DrowsinessSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Necrosis =>
                new NecrosisSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Zombification =>
                new ZombificationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowComplexityChange =>
                new LowComplexityChangeSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedComplexityChange =>
                new MedComplexityChangeSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowPostMortemResistance =>
                new LowPostMortemResistanceSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedPostMortemResistance =>
                new MedPostMortemResistanceSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowViralRegeneration =>
                new LowViralRegenerationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedViralRegeneration =>
                new MedViralRegenerationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowMutationAcceleration =>
                new LowMutationAccelerationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedMutationAcceleration =>
                new MedMutationAccelerationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowPathogenFortress =>
                new LowPathogenFortressSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedPathogenFortress =>
                new MedPathogenFortressSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.LowChemicalAdaptation =>
                new LowChemicalAdaptationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.MedChemicalAdaptation =>
                new MedChemicalAdaptationSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.AggressiveTransmission =>
                new AggressiveTransmissionSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.NeuroSpike =>
                new NeuroSpikeSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.VocalDisruption =>
                new VocalDisruptionSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Asphyxia =>
                new AsphyxiaSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.Blindable =>
                new BlindableSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            VirusSymptom.ParalyzedLegs =>
                new ParalyzedLegsSymptom(EntityManager, _timing, _random, DefaultSymptomWindow),

            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                $"Unknown virus symptom {type}"
            )
        };
    }

    public bool TryGetSymptom<T>(Entity<VirusComponent?> entity, out T? symptom)
    where T : class, IVirusSymptom
    {
        symptom = null;

        if (!Resolve(entity, ref entity.Comp, false))
        {
            _sawmill.Warning($"Entity {entity.Owner} не имеет компонента VirusComponent, невозможно получить симптом {typeof(T).Name}.");
            return default!;
        }

        symptom = entity.Comp.ActiveSymptomInstances.OfType<T>().FirstOrDefault();
        return symptom != null;
    }

    public T EnsureSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
        {
            _sawmill.Warning($"Entity {entity.Owner} не имеет компонента VirusComponent, невозможно добавить симптом {typeof(T).Name}.");
            return default!;
        }

        // Ищем симптом нужного типа
        var existing = entity.Comp.ActiveSymptomInstances.OfType<T>().FirstOrDefault();
        if (existing != null)
            return existing;

        return AddSymptom<T>(entity);
    }

    public T AddSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
        {
            _sawmill.Warning($"Entity {entity.Owner} не имеет компонента VirusComponent, невозможно добавить симптом {typeof(T).Name}.");
            return default!;
        }

        if (entity.Comp.ActiveSymptomInstances == null)
            entity.Comp.ActiveSymptomInstances = new List<IVirusSymptom>();

        // создаём симптом с таймером
        var symptom = (T)Activator.CreateInstance(typeof(T), this, _timing, DefaultSymptomWindow)!;

        if (entity.Comp.ActiveSymptomInstances.Any(s => s.Type == symptom.Type))
            return symptom; // возвращаем существующий симптом, если он уже есть

        entity.Comp.ActiveSymptomInstances.Add(symptom);
        symptom.OnAdded(entity.Owner, entity.Comp);

        _sawmill.Debug($"Добавлен симптом {typeof(T).Name} к сущности {entity.Owner}.");

        return symptom;
    }

    public void RemoveSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
        {
            _sawmill.Warning($"Entity {entity.Owner} не имеет компонента VirusComponent, невозможно удалить симптом {typeof(T).Name}.");
            return;
        }

        if (entity.Comp.ActiveSymptomInstances == null)
            return;

        var symptom = entity.Comp.ActiveSymptomInstances.FirstOrDefault(s => s is T);
        if (symptom == null)
            return;

        symptom.OnRemoved(entity.Owner, entity.Comp);

        entity.Comp.ActiveSymptomInstances.Remove(symptom);

        _sawmill.Debug($"Удалён симптом {typeof(T).Name} у сущности {entity.Owner}.");
    }

}
