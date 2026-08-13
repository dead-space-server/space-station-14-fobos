using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared._Starlight.Antags.Abductor.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Starlight.Antags.Abductor.EntitySystems;

/// <summary>
/// Periodically activates the effect of an abductor gland implanted into a victim.
/// </summary>
public sealed partial class AbductorGlandSystem : EntitySystem
{
    [Dependency] private IGameTiming _time = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedTransformSystem _xformSys = default!;

    private float _delayAccumulator = 0f;
    private readonly Stopwatch _stopwatch = new();
    private readonly DamageSpecifier _passiveHealing = new();
    private readonly Dictionary<AbductorOrganType, TimeSpan> _organCooldowns = new()
    {
        { AbductorOrganType.Health, TimeSpan.FromSeconds(3) },
        { AbductorOrganType.NitrousOxide, TimeSpan.FromSeconds(120) },
        { AbductorOrganType.Gravity, TimeSpan.FromSeconds(60) },
        { AbductorOrganType.Egg, TimeSpan.FromSeconds(120) },
        { AbductorOrganType.Spider, TimeSpan.FromSeconds(240) },
    };
    private const float OrganUpdateInterval = 3f;
    private const double OrganUpdateBudgetMs = 0.5;
    private const int PassiveHealingPerType = -3;

    public override void Initialize()
    {
        base.Initialize();
        foreach (var specif in _proto.EnumeratePrototypes<DamageTypePrototype>())
            _passiveHealing.DamageDict.Add(specif.ID, PassiveHealingPerType);

        _stopwatch.Start();
    }

    public override void Update(float frameTime)
    {
        _delayAccumulator += frameTime;

        if (_delayAccumulator < OrganUpdateInterval)
            return;

        _delayAccumulator = 0f;
        _stopwatch.Restart();

        var query = EntityQueryEnumerator<AbductorVictimComponent>();
        while (query.MoveNext(out var uid, out var victim) && _stopwatch.Elapsed < TimeSpan.FromMilliseconds(OrganUpdateBudgetMs))
        {
            if (victim.Organ == AbductorOrganType.None)
                continue;

            TryActivateOrgan(uid, victim);
        }
    }

    private void TryActivateOrgan(EntityUid uid, AbductorVictimComponent victim)
    {
        if (!_organCooldowns.TryGetValue(victim.Organ, out var cooldown))
            return;

        if (_time.CurTime - victim.LastActivation < cooldown)
            return;

        victim.LastActivation = _time.CurTime;

        switch (victim.Organ)
        {
            case AbductorOrganType.Health:
                _damageable.TryChangeDamage(uid, _passiveHealing);
                break;
            case AbductorOrganType.NitrousOxide:
                HandleNitrousOxideOrgan(uid);
                break;
            case AbductorOrganType.Gravity:
                HandleGravityOrgan(uid);
                break;
            case AbductorOrganType.Egg:
                SpawnAttachedTo("FoodEggChickenFertilized", Transform(uid).Coordinates);
                break;
            case AbductorOrganType.Spider:
                SpawnAttachedTo("MobSpiderlingSpiderAngry", Transform(uid).Coordinates);
                break;
        }
    }

    private void HandleNitrousOxideOrgan(EntityUid uid)
    {
        var mix = _atmos.GetContainingMixture((uid, Transform(uid)), true, true) ?? new();
        mix.AdjustMoles(Gas.NitrousOxide, 30);
        _chat.TryEmoteWithChat(uid, "Cough");
    }

    private void HandleGravityOrgan(EntityUid uid)
    {
        var gravity = SpawnAttachedTo("AbductorGravityGlandGravityWell", Transform(uid).Coordinates);
        _xformSys.SetParent(gravity, uid);
    }
}
