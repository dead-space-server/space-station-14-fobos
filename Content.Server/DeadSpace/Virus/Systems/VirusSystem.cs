// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Necromorphs.InfectionDead.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Humanoid;
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

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Стандартное окно времени проявления симптом.
    /// </summary>
    private TimedWindow _defaultSymptomWindow = default!;
    public readonly ProtoId<TagPrototype> VirusAlwaysInfectableTag = "VirusAlwaysInfectable";
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

        _defaultSymptomWindow = new TimedWindow(15f, 60f, _timing, _random);

        SubscribeLocalEvent<VirusComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VirusComponent, ComponentShutdown>(OnShutdown);

        RashInitialize();
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

    private void UpdateVirus(EntityUid uid, VirusComponent component)
    {
        foreach (var symptom in component.ActiveSymptomInstances)
        {
            symptom.OnUpdate(uid, component);
        }
    }

    private void OnComponentInit(EntityUid uid, VirusComponent component, ComponentInit args)
    {
        RefreshSymptoms((uid, component));

        var whitelist = component.Data.EntityWhitelist ??= new EntityWhitelist();

        whitelist.Components ??= Array.Empty<string>();
        var compList = whitelist.Components.ToHashSet();

        compList.Add("MobState");
        compList.Add("HumanoidAppearance");
        compList.Add("Bloodstream");

        whitelist.Components = compList.ToArray();

        component.VirusUpdateWindow = new TimedWindow(1f, 1f, _timing, _random);
    }

    private void OnShutdown(EntityUid uid, VirusComponent component, ComponentShutdown args)
    {
        foreach (var symptom in component.ActiveSymptomInstances)
        {
            if (symptom.EffectTimedWindow.IsExpired())
            {
                symptom.EffectTimedWindow.Reset();
                symptom.OnRemoved(uid, component);
            }
        }
    }

    public bool HasSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
        {
            _sawmill.Warning($"Entity {entity.Owner} не имеет компонента VirusComponent, невозможно проверить наличие симптома {typeof(T).Name}.");
            return default!;
        }

        return entity.Comp.ActiveSymptomInstances.Any(s => s is T);
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
        var symptom = (T)Activator.CreateInstance(typeof(T), this, _timing, _defaultSymptomWindow)!;

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

            var symptomInstance = CreateSymptomInstance(symptom.Type);

            // Проверяем, есть ли уже экземпляр этого типа симптома
            if (host.Comp.ActiveSymptomInstances.Any(s => s.Type == symptom.Type))
                continue;

            if (symptomInstance.EffectTimedWindow.IsExpired())
                symptomInstance.EffectTimedWindow.Reset();

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

        if (!CanInfect(target, host.Comp) && !_tag.HasTag(target, IgnoreCanInfectTag))
            return;

        if (_tag.HasTag(target, VirusAlwaysInfectableTag))
        {
            InfectEntity((host, host.Comp), target);
            return;
        }

        // Вычисляем шанс заражения
        var chance = GetVirusInfectionChance(target, host.Comp);

        // Бросаем шанс
        if (_random.Prob(chance))
        {
            _sawmill.Debug($"[{host}] заразил [{target}] вирусом {host.Comp.Data.StrainId} (шанс {chance:P0})");
            InfectEntity((host, host.Comp), target);
        }
        else
        {
            _sawmill.Debug($"[{host}] не заразил [{target}] (шанс {chance:P0})");
        }
    }


    private void InfectEntity(Entity<VirusComponent?> source, EntityUid target)
    {
        if (!Resolve(source, ref source.Comp, false))
            return;

        CopyVirusComponent(source, target);

        // Активируем симптомы
        var targetComp = Comp<VirusComponent>(target);
        foreach (var symptom in targetComp.ActiveSymptomInstances)
        {
            symptom.OnAdded(target, targetComp);
        }

        // Если будет логика на клиенте
        // Dirty(target, targetComp);
    }

    /// <summary>
    ///     Возможность заразиться вирусом.
    /// </summary>
    public bool CanInfect(EntityUid target, VirusComponent component)
    {
        if (HasComp<VirusComponent>(target)
            || HasComp<ZombieComponent>(target)
            || HasComp<NecromorfComponent>(target)
            || HasComp<InfectionDeadComponent>(target)
            || HasComp<PendingZombieComponent>(target))
            return false;

        if (!_whitelist.IsWhitelistPass(component.Data.EntityWhitelist, target))
            return false;

        if (TryComp<HumanoidAppearanceComponent>(target, out var humanoid) && !component.Data.SpeciesWhitelist.Contains(_prototype.Index(humanoid.Species)))
            return false;

        return true;
    }

    private float GetVirusInfectionChance(EntityUid target, VirusComponent component)
    {
        var resistanceQuery = new VirusResistanceQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(target, resistanceQuery);

        var finalChance = component.Data.Infectivity * resistanceQuery.TotalCoefficient;

        // от 0 до 100%
        finalChance = Math.Clamp(finalChance, 0f, 1.0f);

        return finalChance;
    }

    private void CopyVirusComponent(Entity<VirusComponent?> source, EntityUid target)
    {
        if (!Resolve(source, ref source.Comp, false))
            return;

        var targetComp = EnsureComp<VirusComponent>(target);

        targetComp.Data = (VirusData)source.Comp.Data.Clone();

        // Dirty(targetComp); // если нужно будет обновить клиент
    }

    private IVirusSymptom CreateSymptomInstance(VirusSymptom type)
    {
        return type switch
        {
            VirusSymptom.Cough => new CoughSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            VirusSymptom.Vomit => new VomitSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            VirusSymptom.Rash => new RashSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            VirusSymptom.Drowsiness => new DrowsinessSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            VirusSymptom.Necrosis => new NecrosisSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            VirusSymptom.Zombification => new ZombificationSymptom(EntityManager, _timing, _random, _defaultSymptomWindow),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown virus symptom {type}")
        };
    }


}
