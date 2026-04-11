using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingSlaveClientSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private const string LayerKey = "ShadowlingSlaveEyes";
    private readonly ResPath _rsiPath = new("/Textures/_DeadSpace/Demons/shadowling.rsi");

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<ShadowlingSlaveComponent, GetStatusIconsEvent>(OnGetSlaveIcon);
        SubscribeLocalEvent<ShadowlingRecruitComponent, GetStatusIconsEvent>(OnGetMasterIcon);
        SubscribeLocalEvent<ShadowlingRevealComponent, GetStatusIconsEvent>(OnGetMasterIcon);

        SubscribeLocalEvent<ShadowlingSlaveComponent, ComponentStartup>(OnSlaveStartup);
        SubscribeLocalEvent<ShadowlingSlaveComponent, ComponentShutdown>(OnSlaveShutdown);
    }

    private void OnSlaveStartup(EntityUid uid, ShadowlingSlaveComponent component, ComponentStartup args)
    {
        UpdateSlaveAppearance(uid, true);
    }

    private void OnSlaveShutdown(EntityUid uid, ShadowlingSlaveComponent component, ComponentShutdown args)
    {
        UpdateSlaveAppearance(uid, false);
    }

    private void UpdateSlaveAppearance(EntityUid uid, bool isSlave)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!isSlave)
        {
            if (sprite.LayerMapTryGet(LayerKey, out var layer))
                sprite.LayerSetVisible(layer, false);
            return;
        }

        var protoId = MetaData(uid).EntityPrototype?.ID;
        if (protoId == null) return;

        if (protoId.Contains("MobXeno") || protoId.Contains("MobIPC") || protoId.Contains("MobDiona"))
            return;

        string state;
        if (protoId.Contains("MobArachnid"))
            state = "shadowling_slave-eyes_arachnid";
        else if (protoId.Contains("MobMoth"))
            state = "shadowling_slave-eyes_moth";
        else if (protoId.Contains("MobVox"))
            state = "shadowling_slave-eyes_vox";
        else
            state = "shadowling_slave-eyes";

        if (!sprite.LayerMapTryGet(LayerKey, out var eyesLayer))
        {
            int targetIndex = 0;
            if (sprite.LayerMapTryGet(HumanoidVisualLayers.Eyes, out var humanoidEyes))
            {
                targetIndex = humanoidEyes + 1;
            }

            eyesLayer = sprite.AddLayer(new SpriteSpecifier.Rsi(_rsiPath, state), targetIndex);
            sprite.LayerMapSet(LayerKey, eyesLayer);
        }
        else
        {
            sprite.LayerSetRSI(eyesLayer, _rsiPath);
            sprite.LayerSetState(eyesLayer, state);
        }

        sprite.LayerSetVisible(eyesLayer, true);
    }

    private void OnGetSlaveIcon(EntityUid uid, ShadowlingSlaveComponent component, ref GetStatusIconsEvent args)
    {
        if (!IsShadowlingFaction()) return;

        if (_prototype.TryIndex<FactionIconPrototype>("ShadowlingSlaveFaction", out var icon))
        {
            args.StatusIcons.Add(icon);
        }
    }

    private void OnGetMasterIcon(EntityUid uid, IComponent component, ref GetStatusIconsEvent args)
    {
        if (!IsShadowlingFaction()) return;

        if (_prototype.TryIndex<FactionIconPrototype>("ShadowlingMasterFaction", out var icon))
        {
            args.StatusIcons.Add(icon);
        }
    }

    private bool IsShadowlingFaction()
    {
        var localPlayer = _player.LocalEntity;
        if (localPlayer == null) return false;

        return HasComp<ShadowlingRecruitComponent>(localPlayer.Value) || 
               HasComp<ShadowlingSlaveComponent>(localPlayer.Value) || 
               HasComp<ShadowlingRevealComponent>(localPlayer.Value);
    }
}