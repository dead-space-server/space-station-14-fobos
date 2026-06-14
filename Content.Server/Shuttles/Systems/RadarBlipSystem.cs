// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Shared.DeadSpace.Shuttles.Components;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server.Shuttles.Systems;

public sealed class RadarBlipSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly TagSystem _tags = default!;

    private const float BlipRadius = 0.5f;

    private readonly Dictionary<RadarConsoleComponent, CachedBlipConfig> _configCache = new();
    private readonly HashSet<EntityUid> _candidates = new();
    private readonly HashSet<Entity<IComponent>> _typedCandidates = new();

    public List<BlipState> CollectSpaceBlips(EntityUid consoleUid, RadarConsoleComponent component, float range)
    {
        var blips = new List<BlipState>();

        if (!component.Advanced)
            return blips;

        if (!TryComp(consoleUid, out TransformComponent? consoleXform) ||
            consoleXform.MapUid == null)
        {
            return blips;
        }

        var config = GetConfig(component);
        var worldPos = _xform.GetWorldPosition(consoleXform);
        var mapId = consoleXform.MapID;

        _candidates.Clear();
        CollectCandidates(mapId, worldPos, range, config);

        foreach (var ent in _candidates)
        {
            if (ent == consoleUid ||
                ent == consoleXform.ParentUid ||
                HasComp<MapComponent>(ent) ||
                HasComp<MapGridComponent>(ent))
            {
                continue;
            }

            if (!TryComp(ent, out TransformComponent? entXform) ||
                entXform.GridUid != null)
            {
                continue;
            }

            if (!TryComp<PhysicsComponent>(ent, out var phys) || !phys.CanCollide)
                continue;

            if (HasBlacklistedComponent(ent, config) || HasBlacklistedTag(ent, config))
                continue;

            if (!TryPickColor(ent, config, out var color))
                continue;

            var entWorldPos = _xform.GetWorldPosition(entXform);
            blips.Add(new BlipState(entWorldPos, color, BlipRadius));
        }

        _candidates.Clear();
        return blips;
    }

    private void CollectCandidates(MapId mapId, Vector2 worldPos, float range, CachedBlipConfig config)
    {
        if (config.AllowedTags.Length == 0 && config.AllowedComponents.Length > 0)
        {
            _typedCandidates.Clear();
            foreach (var (type, _) in config.AllowedComponents)
            {
                _lookup.GetEntitiesInRange(type, new MapCoordinates(worldPos, mapId), range, _typedCandidates, LookupFlags.Uncontained);
            }

            foreach (var ent in _typedCandidates)
            {
                _candidates.Add(ent.Owner);
            }

            _typedCandidates.Clear();
            return;
        }

        _lookup.GetEntitiesInRange(mapId, worldPos, range, _candidates, LookupFlags.Uncontained);
    }

    private CachedBlipConfig GetConfig(RadarConsoleComponent component)
    {
        var fingerprint = GetConfigFingerprint(component);
        if (_configCache.TryGetValue(component, out var cached) && cached.Fingerprint == fingerprint)
            return cached;

        cached = new CachedBlipConfig(
            fingerprint,
            ResolveAllowedEntries(component.AllowedComponents),
            ResolveComponentTypes(component.BlacklistComponents),
            component.AllowedTags.ToArray(),
            component.BlacklistTags.ToArray());

        _configCache[component] = cached;
        return cached;
    }

    private int GetConfigFingerprint(RadarConsoleComponent component)
    {
        var hash = new HashCode();

        foreach (var entry in component.AllowedComponents)
        {
            hash.Add(entry.Component);
            hash.Add(entry.Color);
        }

        foreach (var entry in component.BlacklistComponents)
        {
            hash.Add(entry);
        }

        foreach (var entry in component.AllowedTags)
        {
            hash.Add(entry.Tag);
            hash.Add(entry.Color);
        }

        foreach (var entry in component.BlacklistTags)
        {
            hash.Add(entry);
        }

        return hash.ToHashCode();
    }

    private (Type Type, Color Color)[] ResolveAllowedEntries(List<RadarBlipEntry> entries)
    {
        var result = new List<(Type, Color)>(entries.Count);
        foreach (var entry in entries)
        {
            if (_componentFactory.TryGetRegistration(entry.Component, out var reg))
                result.Add((reg.Type, entry.Color));
            else
                Log.Warning($"[RadarConsole] AllowedComponents: component '{entry.Component}' not found.");
        }

        return result.ToArray();
    }

    private Type[] ResolveComponentTypes(List<string> names)
    {
        var result = new List<Type>(names.Count);
        foreach (var name in names)
        {
            if (_componentFactory.TryGetRegistration(name, out var reg))
                result.Add(reg.Type);
            else
                Log.Warning($"[RadarConsole] Blacklist: component '{name}' not found.");
        }

        return result.ToArray();
    }

    private bool HasBlacklistedComponent(EntityUid ent, CachedBlipConfig config)
    {
        foreach (var type in config.BlacklistComponents)
        {
            if (HasComp(ent, type))
                return true;
        }

        return false;
    }

    private bool HasBlacklistedTag(EntityUid ent, CachedBlipConfig config)
    {
        foreach (var tag in config.BlacklistTags)
        {
            if (_tags.HasTag(ent, tag))
                return true;
        }

        return false;
    }

    private bool TryPickColor(EntityUid ent, CachedBlipConfig config, out Color color)
    {
        foreach (var (type, entryColor) in config.AllowedComponents)
        {
            if (!HasComp(ent, type))
                continue;

            color = entryColor;
            return true;
        }

        foreach (var entry in config.AllowedTags)
        {
            if (!_tags.HasTag(ent, entry.Tag))
                continue;

            color = entry.Color;
            return true;
        }

        if (config.AllowedComponents.Length == 0 && config.AllowedTags.Length == 0)
        {
            color = Color.Yellow;
            return true;
        }

        color = default;
        return false;
    }

    public void ClearCache(RadarConsoleComponent component)
    {
        _configCache.Remove(component);
    }

    private sealed record CachedBlipConfig(
        int Fingerprint,
        (Type Type, Color Color)[] AllowedComponents,
        Type[] BlacklistComponents,
        RadarBlipTagEntry[] AllowedTags,
        string[] BlacklistTags);
}
