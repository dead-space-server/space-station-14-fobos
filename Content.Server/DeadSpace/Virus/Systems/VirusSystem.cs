// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.DeadSpace.Virus.Components;
using Content.Server.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Inventory;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    ///     Окно времени обновления вируса.
    /// </summary>
    private TimedWindow _virusUpdateWindow = default!;

    /// <summary>
    ///     Стандартное окно времени проявления симптом.
    /// </summary>
    private TimedWindow _defaultSymptomWindow = default!;
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

        _virusUpdateWindow = new TimedWindow(1f, 1f, _timing, _random);
        _defaultSymptomWindow = new TimedWindow(15f, 60f, _timing, _random);

        SubscribeLocalEvent<VirusComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VirusComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VirusComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_virusUpdateWindow.IsExpired())
            {
                _virusUpdateWindow.Reset();
                UpdateVirus(uid, component);
            }
        }
    }

    private void UpdateVirus(EntityUid uid, VirusComponent component)
    {
        foreach (var symptom in component.ActiveSymptomInstances)
        {
            if (symptom.EffectTimedWindow.IsExpired())
            {
                symptom.EffectTimedWindow.Reset();
                symptom.DoEffect(uid, component);
            }
        }
    }

    private void OnComponentInit(EntityUid uid, VirusComponent component, ComponentInit args)
    {
        foreach (var symptom in component.ActiveSymptomInstances)
        {
            if (symptom.EffectTimedWindow.IsExpired())
            {
                symptom.EffectTimedWindow.Reset();
                symptom.OnAdded(uid, component);
            }
        }
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

    public T AddSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return default!;

        if (entity.Comp.ActiveSymptomInstances == null)
            entity.Comp.ActiveSymptomInstances = new List<IVirusSymptom>();

        // создаём симптом с таймером
        var symptom = (T)Activator.CreateInstance(typeof(T), this, _timing, _defaultSymptomWindow)!;

        if (entity.Comp.ActiveSymptomInstances.Any(s => s.Type == symptom.Type))
            return symptom; // возвращаем существующий симптом, если он уже есть

        entity.Comp.ActiveSymptomInstances.Add(symptom);
        symptom.OnAdded(entity.Owner, entity.Comp);

        return symptom;
    }

    public void RemoveSymptom<T>(Entity<VirusComponent?> entity)
    where T : IVirusSymptom
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (entity.Comp.ActiveSymptomInstances == null)
            return;

        // Находим первый симптом нужного типа
        var symptom = entity.Comp.ActiveSymptomInstances.FirstOrDefault(s => s is T);
        if (symptom == null)
            return;

        symptom.OnRemoved(entity.Owner, entity.Comp);

        entity.Comp.ActiveSymptomInstances.Remove(symptom);
    }

    private float GetVirusInfectionChance(EntityUid uid, VirusComponent component)
    {
        var chance = zombieComponent.BaseZombieInfectionChance;

        var armorEv = new CoefficientQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(uid, armorEv);

        foreach (var resistanceEffectiveness in zombieComponent.ResistanceEffectiveness.DamageDict)
        {
            if (armorEv.DamageModifiers.Coefficients.TryGetValue(resistanceEffectiveness.Key, out var coefficient))
            {
                // Scale the coefficient by the resistance effectiveness, very descriptive I know
                // For example. With 30% slash resist (0.7 coeff), but only a 60% resistance effectiveness for slash,
                // you'll end up with 1 - (0.3 * 0.6) = 0.82 coefficient, or a 18% resistance
                var adjustedCoefficient = 1 - ((1 - coefficient) * resistanceEffectiveness.Value.Float());
                chance *= adjustedCoefficient;
            }
        }

        var zombificationResistanceEv = new ZombificationResistanceQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(uid, zombificationResistanceEv);
        chance *= zombificationResistanceEv.TotalCoefficient;

        return MathF.Max(chance, zombieComponent.MinZombieInfectionChance);
    }
}
