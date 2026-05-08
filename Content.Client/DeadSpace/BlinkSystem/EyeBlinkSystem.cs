// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Bed.Sleep;
using Content.Shared.Blink;

namespace Content.Client.BlinkSystem;

public sealed class EyeBlinkSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private const string BlinkLayerKey = "humanoid_blink_layer";

    private readonly ResPath _rsiPath = new("/Textures/_DeadSpace/Effects/blink.rsi");

    private readonly Dictionary<EntityUid, (float TimeLeft, bool IsClosed)> _blinkData = new();

    private readonly string[] _skipMarkingKeys = { "Malstrem-malstrem", "Malstrem2-malstrem2", "Terminator-terminator", "Beholder-beholder" };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkComponent, ComponentStartup>(OnBlinkStartup);
        SubscribeLocalEvent<BlinkComponent, ComponentShutdown>(OnBlinkShutdown);
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

        if (!_blinkData.ContainsKey(uid))
            _blinkData[uid] = (_random.NextFloat(30f, 80f), false);
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

    private void OnSleepShutdown(EntityUid uid, SleepingComponent component, ComponentShutdown args)
    {
        if (!_blinkData.TryGetValue(uid, out var data))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(BlinkLayerKey, out var layerIndex))
        {
            sprite.LayerSetVisible(layerIndex, false);
        }
        _blinkData[uid] = (_random.NextFloat(30f, 80f), false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var uid in _blinkData.Keys.ToArray())
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite) || !TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
            {
                _blinkData.Remove(uid);
                continue;
            }

            if (!sprite.LayerMapTryGet(BlinkLayerKey, out var layerIndex))
                continue;

            if (HasSkipMarkings(sprite))
            {
                sprite.LayerSetVisible(layerIndex, false);
                continue;
            }

            var (timeLeft, isClosed) = _blinkData[uid];

            if (TryComp<MobStateComponent>(uid, out var mobState) && (mobState.CurrentState == MobState.Dead || mobState.CurrentState == MobState.Critical))
            {
                sprite.LayerSetVisible(layerIndex, false);
                continue;
            }

            if (HasComp<SleepingComponent>(uid))
            {
                sprite.LayerSetColor(layerIndex, appearance.SkinColor);
                sprite.LayerSetVisible(layerIndex, true);
                continue;
            }

            timeLeft -= frameTime;

            if (timeLeft <= 0)
            {
                if (isClosed)
                {
                    sprite.LayerSetVisible(layerIndex, false);
                    _blinkData[uid] = (_random.NextFloat(30f, 80f), false);
                }
                else
                {
                    sprite.LayerSetColor(layerIndex, appearance.SkinColor);
                    sprite.LayerSetVisible(layerIndex, true);
                    _blinkData[uid] = (1.5f, true);
                }
            }
            else
            {
                _blinkData[uid] = (timeLeft, isClosed);
            }
        }
    }
}