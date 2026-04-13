// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Collections.Generic;
using System.Linq;
using Content.Shared.Bed.Sleep;

namespace Content.Client.BlinkSystem;

public sealed class EyeBlinkSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly ResPath _rsiPath = new("/Textures/_DeadSpace/Effects/blink.rsi");
    private const string LayerKey = "MobHumanBlinkLayer";

    private readonly Dictionary<EntityUid, (float TimeLeft, bool IsClosed, int LayerIndex)> _blinkData = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, ComponentStartup>(OnHumanoidStartup);
        SubscribeLocalEvent<HumanoidAppearanceComponent, ComponentShutdown>(OnHumanoidShutdown);
        SubscribeLocalEvent<SleepingComponent, ComponentShutdown>(OnSleepShutdown);
    }

    private void OnHumanoidStartup(EntityUid uid, HumanoidAppearanceComponent component, ComponentStartup args)
    {
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

        if (!sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var eyeLayer))
            return;

        if (_blinkData.ContainsKey(uid))
            return;

        var targetIndex = eyeLayer + 1;
        var actualIndex = sprite.AddLayer(new SpriteSpecifier.Rsi(_rsiPath, state), targetIndex);

        sprite.LayerSetVisible(actualIndex, false);
        sprite.LayerSetColor(actualIndex, component.SkinColor);

        _blinkData[uid] = (_random.NextFloat(20f, 80f), false, actualIndex);
    }

    private void OnHumanoidShutdown(EntityUid uid, HumanoidAppearanceComponent component, ComponentShutdown args)
    {
        _blinkData.Remove(uid);
    }

    private void OnSleepShutdown(EntityUid uid, SleepingComponent component, ComponentShutdown args)
    {
        if (!_blinkData.TryGetValue(uid, out var data))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetVisible(data.LayerIndex, false);
        _blinkData[uid] = (_random.NextFloat(20f, 80f), false, data.LayerIndex);
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

            var (timeLeft, isClosed, layerIndex) = _blinkData[uid];

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
                    _blinkData[uid] = (_random.NextFloat(20f, 80f), false, layerIndex);
                }
                else
                {
                    sprite.LayerSetColor(layerIndex, appearance.SkinColor);
                    sprite.LayerSetVisible(layerIndex, true);
                    _blinkData[uid] = (2f, true, layerIndex);
                }
            }
            else
            {
                _blinkData[uid] = (timeLeft, isClosed, layerIndex);
            }
        }
    }
}
