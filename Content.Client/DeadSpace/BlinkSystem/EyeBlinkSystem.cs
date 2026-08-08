// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared.Bed.Sleep;
using Content.Shared.Blink;

namespace Content.Client.BlinkSystem;

public sealed class EyeBlinkSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private const string BlinkLayerKey = "humanoid_blink_layer";

    private readonly ResPath _rsiPath = new("/Textures/_DeadSpace/Effects/blink.rsi");

    private readonly Dictionary<EntityUid, BlinkData> _blinkData = new();

    private readonly string[] _skipMarkingKeys = 
    {
        "Malstrem-malstrem",
        "Malstrem2-malstrem2",
        "Terminator-terminator",
        "Beholder-beholder",
        "GauzeLefteyePatch-gauze_lefteye_2",
        "GauzeRighteyePatch-gauze_righteye_2",
        "GauzeLefteyePad-gauze_lefteye_1",
        "GauzeRighteyePad-gauze_righteye_1",
        "GauzeBlindfold-gauze_blindfold"
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkComponent, ComponentStartup>(OnBlinkStartup);
        SubscribeLocalEvent<BlinkComponent, ComponentShutdown>(OnBlinkShutdown);
        SubscribeLocalEvent<BlinkComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SleepingComponent, ComponentStartup>(OnSleepStartup);
        SubscribeLocalEvent<SleepingComponent, ComponentShutdown>(OnSleepShutdown);
    }

    private void OnBlinkStartup(EntityUid uid, BlinkComponent component, ComponentStartup args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
            return;

        var meta = MetaData(uid);
        var protoId = meta.EntityPrototype?.ID;
        if (protoId == null) return;

        if (protoId.Contains("MobDiona") || protoId.Contains("MobXenomorph") ||
            protoId.Contains("MobIPC") || protoId.Contains("MobGingerbread") ||
            protoId.Contains("MobSkeleton") || protoId.Contains("MobSlimePerson"))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        string state = "eye_blink";
        if (protoId.Contains("MobVox")) state = "eye_blink_vox";
        else if (protoId.Contains("MobArachnid")) state = "eye_blink_arachnid";
        else if (protoId.Contains("MobMoth")) state = "eye_blink_moth";
        else if (protoId.Contains("MobKobolt") || protoId.Contains("MobReptilian")) state = "eye_blink_reptilian";

        if (!sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var eyeLayerIndex))
            return;

        if (!sprite.LayerMapTryGet(BlinkLayerKey, out _))
        {
            var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(_rsiPath, state), eyeLayerIndex + 2);
            sprite.LayerMapSet(BlinkLayerKey, layer);
        }

        if (sprite.LayerMapTryGet(BlinkLayerKey, out var actualIndex))
        {
            sprite.LayerSetVisible(actualIndex, false);
            sprite.LayerSetColor(actualIndex, appearance.SkinColor);
        }

        if (!_blinkData.TryAdd(uid, new BlinkData()))
            return;

        if (TryComp<MobStateComponent>(uid, out var mobState) &&
            mobState.CurrentState is MobState.Dead or MobState.Critical)
        {
            return;
        }

        if (HasComp<SleepingComponent>(uid))
        {
            var data = _blinkData[uid];
            data.IsClosed = true;
            SetBlinkVisible(uid, !HasSkipMarkings(sprite), sprite, appearance);
            return;
        }

        ScheduleBlink(uid, NextBlinkDelay());
    }

    private bool HasSkipMarkings(SpriteComponent sprite)
    {
        foreach (var key in _skipMarkingKeys)
        {
            if (sprite.LayerMapTryGet(key, out _))
                return true;
        }
        return false;
    }

    private void OnBlinkShutdown(EntityUid uid, BlinkComponent component, ComponentShutdown args)
    {
        _blinkData.Remove(uid);

        if (TryComp<SpriteComponent>(uid, out var sprite) && sprite.LayerMapTryGet(BlinkLayerKey, out var layer))
        {
            sprite.RemoveLayer(layer);
        }
    }

    private void OnMobStateChanged(Entity<BlinkComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_blinkData.TryGetValue(ent, out var data))
            return;

        if (args.NewMobState is MobState.Dead or MobState.Critical)
        {
            InvalidateSchedule(data);
            data.IsClosed = false;
            SetBlinkVisible(ent, false);
            return;
        }

        if (args.OldMobState is not (MobState.Dead or MobState.Critical))
            return;

        if (HasComp<SleepingComponent>(ent))
        {
            data.IsClosed = true;
            if (TryComp<SpriteComponent>(ent, out var sprite))
                SetBlinkVisible(ent, !HasSkipMarkings(sprite), sprite);
            return;
        }

        ScheduleBlink(ent, NextBlinkDelay());
    }

    private void OnSleepStartup(EntityUid uid, SleepingComponent component, ComponentStartup args)
    {
        if (!_blinkData.TryGetValue(uid, out var data))
            return;

        InvalidateSchedule(data);
        data.IsClosed = true;

        if (TryComp<MobStateComponent>(uid, out var mobState) &&
            mobState.CurrentState is MobState.Dead or MobState.Critical)
        {
            SetBlinkVisible(uid, false);
            return;
        }

        if (!TryComp<SpriteComponent>(uid, out var sprite) || HasSkipMarkings(sprite))
        {
            SetBlinkVisible(uid, false, sprite);
            return;
        }

        SetBlinkVisible(uid, true, sprite);
    }

    private void OnSleepShutdown(EntityUid uid, SleepingComponent component, ComponentShutdown args)
    {
        if (!_blinkData.TryGetValue(uid, out var data))
            return;

        data.IsClosed = false;
        SetBlinkVisible(uid, false);

        if (!TryComp<MobStateComponent>(uid, out var mobState) ||
            mobState.CurrentState is not (MobState.Dead or MobState.Critical))
        {
            ScheduleBlink(uid, NextBlinkDelay());
        }
    }

    private void ScheduleBlink(EntityUid uid, TimeSpan delay)
    {
        if (!_blinkData.TryGetValue(uid, out var data))
            return;

        var schedule = ++data.Schedule;
        Timer.Spawn(delay, () => OnBlinkTimer(uid, data, schedule));
    }

    private void OnBlinkTimer(EntityUid uid, BlinkData scheduledData, int schedule)
    {
        if (!_blinkData.TryGetValue(uid, out var data) ||
            !ReferenceEquals(data, scheduledData) ||
            data.Schedule != schedule)
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            !TryComp<HumanoidAppearanceComponent>(uid, out var appearance) ||
            !sprite.LayerMapTryGet(BlinkLayerKey, out var layerIndex))
        {
            _blinkData.Remove(uid);
            return;
        }

        if (TryComp<MobStateComponent>(uid, out var mobState) &&
            mobState.CurrentState is MobState.Dead or MobState.Critical)
        {
            data.IsClosed = false;
            sprite.LayerSetVisible(layerIndex, false);
            return;
        }

        if (HasComp<SleepingComponent>(uid))
        {
            data.IsClosed = true;
            SetBlinkVisible(uid, !HasSkipMarkings(sprite), sprite, appearance);
            return;
        }

        if (data.IsClosed)
        {
            data.IsClosed = false;
            sprite.LayerSetVisible(layerIndex, false);
            ScheduleBlink(uid, NextBlinkDelay());
            return;
        }

        if (HasSkipMarkings(sprite))
        {
            sprite.LayerSetVisible(layerIndex, false);
            ScheduleBlink(uid, NextBlinkDelay());
            return;
        }

        data.IsClosed = true;
        sprite.LayerSetColor(layerIndex, appearance.SkinColor);
        sprite.LayerSetVisible(layerIndex, true);
        ScheduleBlink(uid, TimeSpan.FromSeconds(1.5));
    }

    private void SetBlinkVisible(EntityUid uid, bool visible, SpriteComponent? sprite = null,
        HumanoidAppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref sprite, false) ||
            !sprite.LayerMapTryGet(BlinkLayerKey, out var layerIndex))
            return;

        if (visible && Resolve(uid, ref appearance, false))
            sprite.LayerSetColor(layerIndex, appearance.SkinColor);

        sprite.LayerSetVisible(layerIndex, visible);
    }

    private TimeSpan NextBlinkDelay()
    {
        return TimeSpan.FromSeconds(_random.NextFloat(30f, 80f));
    }

    private static void InvalidateSchedule(BlinkData data)
    {
        data.Schedule++;
    }

    private sealed class BlinkData
    {
        public int Schedule;
        public bool IsClosed;
    }
}
