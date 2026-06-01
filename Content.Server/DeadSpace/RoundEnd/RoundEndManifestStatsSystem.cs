// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.GameTicking.Events;
using Content.Server.Cloning;
using Content.Shared.Body.Components;
using Content.Shared.Chat;
using Content.Shared.Cloning;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Gibbing;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Roles;
using Robust.Server.GameStates;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.RoundEnd;

public sealed class RoundEndManifestStatsSystem : EntitySystem
{
    private static readonly ProtoId<CloningSettingsPrototype> DisplayCloneSettings = "RoundEndManifestDisplayClone";

    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    private const int MinQuoteLength = 8;
    private const int MaxQuoteLength = 160;
    private const int SourceParentSearchDepth = 6;

    private readonly Dictionary<EntityUid, string> _lastQuoteByMind = new();
    private readonly Dictionary<EntityUid, ManifestKdaStats> _statsByMind = new();
    private readonly Dictionary<EntityUid, Dictionary<EntityUid, FixedPoint2>> _damageByTarget = new();
    private readonly Dictionary<EntityUid, EntityUid> _displaySnapshotByMind = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAdded);
        SubscribeLocalEvent<MindContainerComponent, BeingGibbedEvent>(OnMindBeingGibbed);
        SubscribeLocalEvent<MobStateComponent, DamageChangedEvent>(OnDamageChanged, before: [typeof(MobThresholdSystem)]);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    public RoundEndManifestStats GetManifestStats(EntityUid mindId)
    {
        _statsByMind.TryGetValue(mindId, out var stats);
        return new RoundEndManifestStats(GetQuote(mindId), stats.Kills, stats.Assists);
    }

    public EntityUid? GetDisplaySnapshot(EntityUid mindId)
    {
        if (!_displaySnapshotByMind.TryGetValue(mindId, out var snapshot))
            return null;

        if (!TerminatingOrDeleted(snapshot))
            return snapshot;

        _displaySnapshotByMind.Remove(mindId);
        return null;
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        Reset();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        Reset();
    }

    private void Reset()
    {
        foreach (var snapshot in _displaySnapshotByMind.Values)
        {
            if (!TerminatingOrDeleted(snapshot))
                Del(snapshot);
        }

        _lastQuoteByMind.Clear();
        _statsByMind.Clear();
        _damageByTarget.Clear();
        _displaySnapshotByMind.Clear();
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        if (!TryGetPlayerMind(args.Source, out var mindId, out var mind) ||
            !IsCharacterSpeechSource(args.Source, mind))
        {
            return;
        }

        var quote = SanitizeQuote(args.Message);
        if (quote == null)
            return;

        _lastQuoteByMind[mindId] = quote;
    }

    private void OnRoleAdded(RoleAddedEvent args)
    {
        if (!IsAntagPlayerMind(args.MindId, args.Mind))
            return;

        EnsureDisplaySnapshot(args.MindId, args.Mind);
    }

    private void OnMindBeingGibbed(EntityUid uid, MindContainerComponent component, BeingGibbedEvent args)
    {
        if (!HasComp<BodyComponent>(uid) ||
            !TryGetPlayerMind(uid, out var mindId, out _))
        {
            return;
        }

        TryCreateDisplaySnapshot(mindId, uid, replaceExisting: false);
    }

    private bool EnsureDisplaySnapshot(EntityUid mindId, MindComponent mind)
    {
        if (_displaySnapshotByMind.TryGetValue(mindId, out var oldSnapshot) && !TerminatingOrDeleted(oldSnapshot))
            return true;

        if (GetSnapshotSourceBody(mind) is not { } source)
            return false;

        return TryCreateDisplaySnapshot(mindId, source, replaceExisting: false);
    }

    private EntityUid? GetSnapshotSourceBody(MindComponent mind)
    {
        if (IsSnapshotSourceBody(mind.OwnedEntity))
            return mind.OwnedEntity;

        if (TryGetEntity(mind.OriginalOwnedEntity, out var originalEntity) &&
            IsSnapshotSourceBody(originalEntity))
        {
            return originalEntity.Value;
        }

        return null;
    }

    private bool IsSnapshotSourceBody(EntityUid? uid)
    {
        return uid != null &&
               !TerminatingOrDeleted(uid.Value) &&
               HasComp<BodyComponent>(uid.Value);
    }

    private bool TryCreateDisplaySnapshot(EntityUid mindId, EntityUid source, bool replaceExisting)
    {
        if (_displaySnapshotByMind.TryGetValue(mindId, out var oldSnapshot))
        {
            if (!TerminatingOrDeleted(oldSnapshot))
            {
                if (!replaceExisting)
                    return true;

                Del(oldSnapshot);
            }

            _displaySnapshotByMind.Remove(mindId);
        }

        if (!_cloning.TryCloning(source, null, DisplayCloneSettings, out var snapshot) || snapshot == null)
            return false;

        _displaySnapshotByMind[mindId] = snapshot.Value;
        AddDisplaySnapshotPvsOverride(snapshot.Value);
        return true;
    }

    private void AddDisplaySnapshotPvsOverride(EntityUid uid)
    {
        AddDisplaySnapshotPvsOverride(uid, new HashSet<EntityUid>());
    }

    private void AddDisplaySnapshotPvsOverride(EntityUid uid, HashSet<EntityUid> visited)
    {
        if (!visited.Add(uid) || TerminatingOrDeleted(uid))
            return;

        _pvsOverride.AddGlobalOverride(uid);

        if (!TryComp<ContainerManagerComponent>(uid, out var containers))
            return;

        foreach (var container in containers.Containers.Values)
        {
            foreach (var contained in container.ContainedEntities)
            {
                AddDisplaySnapshotPvsOverride(contained, visited);
            }
        }
    }

    private void OnDamageChanged(EntityUid uid, MobStateComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
        {
            if (args.Damageable.TotalDamage == FixedPoint2.Zero)
                _damageByTarget.Remove(uid);

            return;
        }

        if (!TryGetPlayerMind(uid, out var targetMindId, out _))
        {
            _damageByTarget.Remove(uid);
            return;
        }

        var delta = args.DamageDelta.GetTotal();

        if (!args.DamageIncreased)
        {
            if (args.Damageable.TotalDamage == FixedPoint2.Zero)
            {
                _damageByTarget.Remove(uid);
                return;
            }

            ReduceDamageContributors(uid, -delta);
            return;
        }

        if (delta <= FixedPoint2.Zero)
            return;

        if (!TryGetDamageSourceMind(args.Origin, out var sourceMindId, out var sourceMind) ||
            sourceMindId == targetMindId ||
            !IsAntagPlayerMind(sourceMindId, sourceMind))
        {
            return;
        }

        var sourceDamage = _damageByTarget.GetOrNew(uid);
        sourceDamage[sourceMindId] = sourceDamage.GetValueOrDefault(sourceMindId) + delta;
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState >= args.NewMobState)
            return;

        var uid = args.Target;
        if (!TryGetPlayerMind(uid, out var targetMindId, out _))
        {
            _damageByTarget.Remove(uid);
            return;
        }

        EntityUid? killerMind = null;
        if (TryGetDamageSourceMind(args.Origin, out var originMindId, out var originMind))
        {
            if (originMindId != targetMindId && IsAntagPlayerMind(originMindId, originMind))
                killerMind = originMindId;
        }
        else if (TryGetLargestAntagContributor(uid, targetMindId, out var largestContributor))
        {
            killerMind = largestContributor;
        }

        if (killerMind == null)
        {
            _damageByTarget.Remove(uid);
            return;
        }

        AddKill(killerMind.Value);
        AddAssists(uid, targetMindId, killerMind.Value);
        _damageByTarget.Remove(uid);
    }

    private void AddKill(EntityUid mindId)
    {
        var stats = _statsByMind.GetValueOrDefault(mindId);
        stats.Kills++;
        _statsByMind[mindId] = stats;
    }

    private void AddAssists(EntityUid target, EntityUid targetMindId, EntityUid killerMindId)
    {
        if (!_damageByTarget.TryGetValue(target, out var sources))
            return;

        foreach (var (sourceMindId, damage) in sources)
        {
            if (damage <= FixedPoint2.Zero ||
                sourceMindId == targetMindId ||
                sourceMindId == killerMindId ||
                !TryComp<MindComponent>(sourceMindId, out var sourceMind) ||
                !IsAntagPlayerMind(sourceMindId, sourceMind))
            {
                continue;
            }

            var stats = _statsByMind.GetValueOrDefault(sourceMindId);
            stats.Assists++;
            _statsByMind[sourceMindId] = stats;
        }
    }

    private bool TryGetLargestAntagContributor(EntityUid target, EntityUid targetMindId, out EntityUid sourceMindId)
    {
        sourceMindId = default;
        if (!_damageByTarget.TryGetValue(target, out var sources))
            return false;

        var largestDamage = FixedPoint2.Zero;
        var found = false;

        foreach (var (candidateMindId, damage) in sources)
        {
            if (damage <= largestDamage ||
                candidateMindId == targetMindId ||
                !TryComp<MindComponent>(candidateMindId, out var candidateMind) ||
                !IsAntagPlayerMind(candidateMindId, candidateMind))
            {
                continue;
            }

            sourceMindId = candidateMindId;
            largestDamage = damage;
            found = true;
        }

        return found;
    }

    private void ReduceDamageContributors(EntityUid target, FixedPoint2 healing)
    {
        if (healing <= FixedPoint2.Zero || !_damageByTarget.TryGetValue(target, out var sources))
            return;

        var totalTrackedDamage = FixedPoint2.Zero;
        foreach (var damage in sources.Values)
        {
            if (damage > FixedPoint2.Zero)
                totalTrackedDamage += damage;
        }

        if (totalTrackedDamage <= healing)
        {
            _damageByTarget.Remove(target);
            return;
        }

        var sourceMindIds = new EntityUid[sources.Count];
        sources.Keys.CopyTo(sourceMindIds, 0);

        foreach (var sourceMindId in sourceMindIds)
        {
            var damage = sources[sourceMindId];
            var reduction = damage / totalTrackedDamage * healing;
            var remaining = damage - reduction;
            if (remaining <= FixedPoint2.Zero)
                sources.Remove(sourceMindId);
            else
                sources[sourceMindId] = remaining;
        }

        if (sources.Count == 0)
            _damageByTarget.Remove(target);
    }

    private string GetQuote(EntityUid mindId)
    {
        if (!_lastQuoteByMind.TryGetValue(mindId, out var quote))
            return Loc.GetString("round-end-summary-window-antag-manifest-quote-fallback");

        return quote;
    }

    private string? SanitizeQuote(string message)
    {
        var quote = FormattedMessage.RemoveMarkupPermissive(message).Trim();
        quote = string.Join(" ", quote.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));

        if (quote.Length < MinQuoteLength || CountMeaningfulCharacters(quote) < 3)
            return null;

        if (quote.Length > MaxQuoteLength)
            quote = $"{quote[..MaxQuoteLength].TrimEnd()}...";

        return quote;
    }

    private static int CountMeaningfulCharacters(string quote)
    {
        var count = 0;
        foreach (var character in quote)
        {
            if (char.IsLetterOrDigit(character))
                count++;
        }

        return count;
    }

    private bool IsCharacterSpeechSource(EntityUid source, MindComponent mind)
    {
        return mind.OwnedEntity == source && !HasComp<GhostComponent>(source);
    }

    private bool TryGetDamageSourceMind(EntityUid? source, out EntityUid mindId, out MindComponent mind)
    {
        mindId = default;
        mind = default!;

        if (source == null)
            return false;

        if (TryGetMind(source.Value, out mindId, out mind))
            return true;

        if (TryGetProjectileSourceMind(source.Value, out mindId, out mind))
            return true;

        var current = source.Value;
        for (var i = 0; i < SourceParentSearchDepth; i++)
        {
            if (!TryComp(current, out TransformComponent? transform))
                return false;

            var parent = transform.ParentUid;
            if (parent == current)
                return false;

            if (TryGetMind(parent, out mindId, out mind))
                return true;

            if (TryGetProjectileSourceMind(parent, out mindId, out mind))
                return true;

            current = parent;
        }

        return false;
    }

    private bool TryGetProjectileSourceMind(EntityUid uid, out EntityUid mindId, out MindComponent mind)
    {
        mindId = default;
        mind = default!;

        if (!TryComp<ProjectileComponent>(uid, out var projectile))
            return false;

        if (projectile.Shooter != null && TryGetMind(projectile.Shooter.Value, out mindId, out mind))
            return true;

        return projectile.Weapon != null && TryGetMind(projectile.Weapon.Value, out mindId, out mind);
    }

    private bool TryGetMind(EntityUid uid, out EntityUid mindId, out MindComponent mind)
    {
        mindId = default;
        mind = default!;

        if (!TryComp<MindContainerComponent>(uid, out var mindContainer) ||
            mindContainer.Mind == null)
        {
            return false;
        }

        var mindEntity = mindContainer.Mind.Value;
        if (!TryComp<MindComponent>(mindEntity, out var mindComponent))
            return false;

        mindId = mindEntity;
        mind = mindComponent;
        return true;
    }

    private bool TryGetPlayerMind(EntityUid uid, out EntityUid mindId, out MindComponent mind)
    {
        if (!TryGetMind(uid, out mindId, out mind) || !IsPlayerMind(mind))
            return false;

        return true;
    }

    private bool IsAntagPlayerMind(EntityUid mindId, MindComponent mind)
    {
        return IsPlayerMind(mind) && _roles.MindIsAntagonist(mindId);
    }

    private static bool IsPlayerMind(MindComponent mind)
    {
        return mind.UserId != null || mind.OriginalOwnerUserId != null;
    }
}

public readonly record struct RoundEndManifestStats(string Quote, int Kills, int Assists);

internal struct ManifestKdaStats
{
    public int Kills;
    public int Assists;
}
