using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared._Frostheim.Supermatter.Components;
using Content.Shared._Frostheim.Supermatter.Data;
using Content.Shared._Frostheim.Supermatter.Systems;
using Content.Shared.Atmos;
using Content.Shared.Explosion.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Radiation.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server._Frostheim.Supermatter.Systems;

[UsedImplicitly]
public sealed class SupermatterSystem : SharedSupermatterSystem
{
    private static readonly ProtoId<TagPrototype> TagSmImmune = "SMImmune";

    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SupermatterComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SupermatterComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<SupermatterComponent, InteractHandEvent>(OnHandInteract);
        SubscribeLocalEvent<SupermatterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SupermatterComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(EntityUid uid, SupermatterComponent component, ComponentInit args)
    {
    }

    private void OnComponentRemove(EntityUid uid, SupermatterComponent component, ComponentRemove args)
    {
        _ambient.SetAmbience(uid, false);
        _audio.Stop(component.AudioEntity);
    }

    private void OnMapInit(EntityUid uid, SupermatterComponent component, MapInitEvent args)
    {
        _ambient.SetAmbience(uid, true);

        var mixture = _atmosphere.GetContainingMixture(uid, true, true);
        mixture?.AdjustMoles(Gas.Oxygen, Atmospherics.OxygenMolesStandard);
        mixture?.AdjustMoles(Gas.Nitrogen, Atmospherics.NitrogenMolesStandard);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<SupermatterComponent, ExplosiveComponent, RadiationSourceComponent>();
        while (query.MoveNext(out var uid, out var supermatter, out var xplode, out var rads))
        {
            var mixture = _atmosphere.GetContainingMixture(uid, true, true);
            HandleOutput(uid, frameTime, supermatter, rads, mixture);
            HandleDamage(uid, frameTime, supermatter, xplode, mixture);
        }
    }

    private void HandleOutput(
        EntityUid uid,
        float frameTime,
        SupermatterComponent? sMcomponent = null,
        RadiationSourceComponent? radcomponent = null,
        GasMixture? mixture = null)
    {
        if (!Resolve(uid, ref sMcomponent, ref radcomponent))
            return;

        sMcomponent.AtmosUpdateAccumulator += frameTime;

        if (!(sMcomponent.AtmosUpdateAccumulator > sMcomponent.AtmosUpdateTimer) || mixture is null)
            return;

        sMcomponent.AtmosUpdateAccumulator -= sMcomponent.AtmosUpdateTimer;

        var absorbedGas = mixture.Remove(sMcomponent.GasEfficiency * mixture.TotalMoles);
        var absorbedTotalMoles = absorbedGas.TotalMoles;

        if (!(absorbedTotalMoles > 0f))
            return;

        var gasEffect = sMcomponent.GasDataFields;

        // Calculate gas ratios (percentage of each gas in mixture)
        var gasRatios = new Dictionary<Gas, float>();
        foreach (var gas in sMcomponent.GasStorage.Keys)
        {
            gasRatios[gas] = Math.Clamp(absorbedGas.GetMoles(gas) / absorbedTotalMoles, 0, 1);
        }

        // TG station gas effect calculations
        var powerTransmission = gasRatios.Sum(g => gasRatios[g.Key] * gasEffect[g.Key].PowerTransmission);
        var heatModifier = gasRatios.Sum(g => gasRatios[g.Key] * gasEffect[g.Key].HeatModifier);
        var heatResistance = gasRatios.Sum(g => gasRatios[g.Key] * gasEffect[g.Key].HeatResistance);
        var heatPowerGeneration = gasRatios.Sum(g => gasRatios[g.Key] * gasEffect[g.Key].HeatPowerGeneration);
        var powerlossInhibition = gasRatios.Sum(g => gasRatios[g.Key] * gasEffect[g.Key].PowerlossInhibition);

        // Store for damage calculations
        sMcomponent.DynamicHeatResistance = 1f + heatResistance;

        // Waste multiplier: TG uses 1 + heatModifier, clamped min 0.5
        var wasteMultiplier = Math.Max(1f + heatModifier, 0.5f);

        // Matter power conversion (from consumed entities)
        if (sMcomponent.MatterPower != 0)
        {
            var removedMatter = Math.Max(sMcomponent.MatterPower / sMcomponent.MatterPowerConversion, 40);
            sMcomponent.Power = Math.Max(sMcomponent.Power + removedMatter, 0);
            sMcomponent.MatterPower = Math.Max(sMcomponent.MatterPower - removedMatter, 0);
        }

        // Heat power generation - clamped to [0,1] so negative gases (N2) only nullify gain, not drain power
        var clampedPowerRatio = Math.Clamp(heatPowerGeneration, 0f, 1f);
        var heatPowerGain = clampedPowerRatio * absorbedGas.Temperature * sMcomponent.HeatPowerScaling;
        sMcomponent.Power = Math.Max(sMcomponent.Power + heatPowerGain, 0);

        // Radiation intensity based on power and transmission
        radcomponent.Intensity = sMcomponent.Power * Math.Max(0, 1f + powerTransmission) * 0.003f;

        // Device energy for waste calculations
        var deviceEnergy = sMcomponent.Power * sMcomponent.ReactionPowerModifier;

        // TG waste gas release formulas
        // Temperature increase
        absorbedGas.Temperature += deviceEnergy * wasteMultiplier / sMcomponent.ThermalReleaseModifier;
        absorbedGas.Temperature = Math.Max(0, Math.Min(absorbedGas.Temperature, sMcomponent.HeatThreshold * wasteMultiplier));

        // Plasma release
        absorbedGas.AdjustMoles(
            Gas.Plasma,
            Math.Max(deviceEnergy * wasteMultiplier / sMcomponent.PlasmaReleaseModifier, 0f));

        // Oxygen release
        absorbedGas.AdjustMoles(
            Gas.Oxygen,
            Math.Max(
                (deviceEnergy + absorbedGas.Temperature * wasteMultiplier - Atmospherics.T0C) /
                sMcomponent.OxygenReleaseModifier,
                0f));

        _atmosphere.Merge(mixture, absorbedGas);

        // TG power decay: cubic below threshold, linear above
        // Powerloss inhibition from CO2 negates decay
        var effectiveInhibition = Math.Clamp(powerlossInhibition, 0f, 1f);
        var decayMultiplier = 1f - effectiveInhibition;

        if (sMcomponent.Power > sMcomponent.PowerPenaltyThreshold)
        {
            // Linear decay above threshold
            var linearDecay = sMcomponent.Power * sMcomponent.PowerlossLinearRate;
            sMcomponent.Power = Math.Max(sMcomponent.Power - linearDecay * decayMultiplier, 0f);
        }
        else
        {
            // Cubic decay below threshold
            var cubicDecay = (float)Math.Pow(sMcomponent.Power / sMcomponent.PowerlossCubicDivisor, 3);
            sMcomponent.Power = Math.Max(sMcomponent.Power - cubicDecay * decayMultiplier, 0f);
        }
    }

    private void HandleDamage(
        EntityUid uid,
        float frameTime,
        SupermatterComponent? sMcomponent = null,
        ExplosiveComponent? xplode = null,
        GasMixture? mixture = null)
    {
        if (!Resolve(uid, ref sMcomponent, ref xplode))
            return;

        var xform = Transform(uid);
        var indices = _xform.GetGridOrMapTilePosition(uid, xform);

        sMcomponent.DamageUpdateAccumulator += frameTime;
        sMcomponent.YellAccumulator += frameTime;

        if (!(sMcomponent.DamageUpdateAccumulator > sMcomponent.DamageUpdateTimer))
            return;

        sMcomponent.DamageArchived = sMcomponent.Damage;

        // Check if exposed to space (no grid or no atmosphere)
        var isSpaced = !xform.GridUid.HasValue || mixture is null || mixture.TotalMoles == 0f;

        if (isSpaced)
        {
            // TG: Space damage = internal_energy * 0.000125, cap 1.0
            var spaceDamage = Math.Min(
                sMcomponent.Power * sMcomponent.SpaceDamageMultiplier,
                sMcomponent.MaxSpaceExposureDamage);
            sMcomponent.Damage += spaceDamage;
        }
        else if (mixture != null)
        {
            // TG temperature limit = (T0C + HeatPenaltyThreshold) * DynamicHeatResistance
            var tempLimit = (Atmospherics.T0C + sMcomponent.HeatPenaltyThreshold) * sMcomponent.DynamicHeatResistance;

            var heatDamage = Math.Max(
                (mixture.Temperature - tempLimit) / sMcomponent.HeatDamageDivisor, 0f);

            var powerDamage = Math.Max(
                (sMcomponent.Power - sMcomponent.PowerPenaltyThreshold) / sMcomponent.PowerDamageDivisor, 0f);

            var moleDamage = Math.Max(
                (mixture.TotalMoles - sMcomponent.MolePenaltyThreshold) / sMcomponent.MoleDamageDivisor, 0f);

            sMcomponent.Damage += heatDamage + powerDamage + moleDamage;

            // Cap max damage increase per tick
            sMcomponent.Damage = Math.Min(
                sMcomponent.DamageArchived + sMcomponent.DamageHardcap * sMcomponent.ExplosionPoint,
                sMcomponent.Damage);

            // TG Healing: when temperature is below limit and has gas
            // Healing = (temperature - temp_limit) / 6000 (negative value = healing), cap -0.1
            if (mixture.Temperature < tempLimit && mixture.TotalMoles > 0)
            {
                var healing = Math.Max(
                    (mixture.Temperature - tempLimit) / sMcomponent.HealingDivisor,
                    -sMcomponent.MaxHealingPerTick);
                sMcomponent.Damage = Math.Max(sMcomponent.Damage + healing, 0f);
            }

            // Check for adjacent space tiles
            var query = _atmosphere.GetAdjacentTileMixtures(xform.GridUid!.Value, indices);
            while (query.MoveNext(out var mix))
            {
                if (mix.TotalMoles != 0)
                    continue;

                // Space exposure from adjacent vacuum
                var spaceDamage = Math.Min(
                    sMcomponent.Power * sMcomponent.SpaceDamageMultiplier,
                    sMcomponent.MaxSpaceExposureDamage);
                sMcomponent.Damage += spaceDamage;
                break;
            }
        }

        HandleSoundLoop(uid, sMcomponent);

        if (sMcomponent.Damage > sMcomponent.ExplosionPoint)
        {
            Delamination(uid, frameTime, sMcomponent, xplode, mixture);
            return;
        }

        if (sMcomponent.Damage > sMcomponent.WarningPoint)
        {
            var integrity = GetIntegrity(sMcomponent.Damage, sMcomponent.ExplosionPoint);
            if (sMcomponent.YellAccumulator >= sMcomponent.YellTimer)
            {
                if (sMcomponent.Damage > sMcomponent.EmergencyPoint && sMcomponent.Damage >= sMcomponent.DamageArchived)
                {
                    _chat.TrySendInGameICMessage(
                        uid,
                        Loc.GetString("supermatter-danger-message", ("integrity", integrity.ToString("0.00"))),
                        InGameICChatType.Speak,
                        hideChat: true);
                }
                else if (sMcomponent.Damage > sMcomponent.EmergencyPoint)
                {
                    _chat.TrySendInGameICMessage(
                        uid,
                        Loc.GetString("supermatter-recovery-alert", ("integrity", integrity.ToString("0.00"))),
                        InGameICChatType.Speak,
                        hideChat: true);
                }
                else if (sMcomponent.Damage >= sMcomponent.DamageArchived)
                {
                    _chat.TrySendInGameICMessage(
                        uid,
                        Loc.GetString("supermatter-warning-message", ("integrity", integrity.ToString("0.00"))),
                        InGameICChatType.Speak,
                        hideChat: true);
                }
                else
                {
                    _chat.TrySendInGameICMessage(
                        uid,
                        Loc.GetString("supermatter-safe-alert", ("integrity", integrity.ToString("0.00"))),
                        InGameICChatType.Speak,
                        hideChat: true);
                }

                sMcomponent.YellAccumulator = 0;
            }
        }

        sMcomponent.DamageUpdateAccumulator -= sMcomponent.DamageUpdateTimer;
    }

    private float GetIntegrity(float damage, float explosionPoint)
    {
        var integrity = damage / explosionPoint;
        integrity = (float) Math.Round(100 - integrity * 100, 2);
        integrity = integrity < 0 ? 0 : integrity;
        return integrity;
    }

    private void Delamination(
        EntityUid uid,
        float frameTime,
        SupermatterComponent? sMcomponent = null,
        ExplosiveComponent? xplode = null,
        GasMixture? mixture = null)
    {
        if (!Resolve(uid, ref sMcomponent, ref xplode))
            return;

        var xform = Transform(uid);

        if (!sMcomponent.FinalCountdown)
        {
            _chat.TrySendInGameICMessage(
                uid,
                Loc.GetString("supermatter-delamination-default"),
                InGameICChatType.Speak,
                hideChat: true);
        }

        sMcomponent.FinalCountdown = true;

        sMcomponent.DelamTimerAccumulator += frameTime;
        sMcomponent.SpeakAccumulator += frameTime;
        var roundSeconds = sMcomponent.DelamTimerTimer - (int) Math.Floor(sMcomponent.DelamTimerAccumulator);

        if (roundSeconds >= sMcomponent.YellDelam && sMcomponent.SpeakAccumulator >= sMcomponent.YellDelam)
        {
            sMcomponent.SpeakAccumulator -= sMcomponent.YellDelam;
            _chat.TrySendInGameICMessage(
                uid,
                Loc.GetString("supermatter-seconds-before-delam", ("Seconds", roundSeconds)),
                InGameICChatType.Speak,
                hideChat: true);
        }
        else if (roundSeconds < sMcomponent.YellDelam && sMcomponent.SpeakAccumulator >= 1)
        {
            sMcomponent.SpeakAccumulator -= 1;
            _chat.TrySendInGameICMessage(
                uid,
                Loc.GetString("supermatter-seconds-before-delam", ("Seconds", roundSeconds)),
                InGameICChatType.Speak,
                hideChat: true);
        }

        if (!(sMcomponent.DelamTimerAccumulator >= sMcomponent.DelamTimerTimer))
            return;

        _explosion.TriggerExplosive(
            uid,
            explosive: xplode,
            totalIntensity: sMcomponent.TotalIntensity,
            radius: sMcomponent.Radius,
            user: uid);

        _audio.Stop(sMcomponent.AudioEntity);
        _ambient.SetAmbience(uid, false);

        sMcomponent.FinalCountdown = false;
    }

    private void HandleSoundLoop(EntityUid uid, SupermatterComponent sMcomponent)
    {
        var isAggressive = sMcomponent.Damage > sMcomponent.WarningPoint;
        var isDelamming = sMcomponent.Damage > sMcomponent.ExplosionPoint;

        if (!isAggressive && !isDelamming)
        {
            _audio.Stop(sMcomponent.AudioEntity);
            return;
        }

        var smSound = isDelamming ? SuperMatterSound.Delam : SuperMatterSound.Aggressive;

        if (sMcomponent.SmSound == smSound)
            return;

        _audio.Stop(sMcomponent.AudioEntity);

        var sounds = isDelamming ? sMcomponent.DelamAlarm : sMcomponent.DelamSound;
        var sound = _audio.ResolveSound(sounds);
        var param = sounds.Params.WithLoop(true).WithVolume(-5f).WithMaxDistance(20f);
        var playing = _audio.PlayPvs(sound, uid, param);
        if (playing != null)
            sMcomponent.AudioEntity = playing.Value.Entity;
        sMcomponent.SmSound = smSound;
    }

    private bool CannotDestroy(EntityUid uid)
    {
        var isStatic = false;
        var tag = _tag.HasTag(uid, TagSmImmune);

        if (TryComp<PhysicsComponent>(uid, out var physicsComp))
            isStatic = physicsComp.BodyType == BodyType.Static;

        return tag || isStatic;
    }

    private void OnCollideEvent(EntityUid uid, SupermatterComponent supermatter, ref StartCollideEvent args)
    {
        var target = args.OtherEntity;

        if (!_whitelist.IsValid(supermatter.Whitelist, target) || CannotDestroy(target) || _container.IsEntityInContainer(uid))
            return;

        if (TryComp<SupermatterFoodComponent>(target, out var supermatterFood))
            supermatter.Power += supermatterFood.Energy;
        else if (TryComp<ProjectileComponent>(target, out var projectile))
            supermatter.Power += (float) projectile.Damage.GetTotal();
        else
            supermatter.Power++;

        supermatter.MatterPower += HasComp<MobStateComponent>(target) ? 200 : 0;

        if (!HasComp<ProjectileComponent>(target))
        {
            Spawn("Ash", Transform(target).Coordinates);
            _audio.PlayPvs(supermatter.DustSound, uid);
        }

        QueueDel(target);
    }

    private void OnHandInteract(EntityUid uid, SupermatterComponent supermatter, InteractHandEvent args)
    {
        var target = args.User;

        if (_tag.HasTag(target, TagSmImmune))
            return;

        supermatter.MatterPower += 200;
        Spawn("Ash", Transform(target).Coordinates);
        _audio.PlayPvs(supermatter.DustSound, uid);
        QueueDel(target);
    }
}
