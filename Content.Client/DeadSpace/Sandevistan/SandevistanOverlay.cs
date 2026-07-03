// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System;
using Content.Shared.CCVar;
using Content.Shared.DeadSpace.Sandevistan;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.DeadSpace.Sandevistan;

public sealed class SandevistanOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SandevistanOverlay";
    private const float FadeDuration = 2.5f;

    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;
    private float _intensity;
    private float _motionScale = 1f;

    public SandevistanOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index(Shader).InstanceUnique();
        ZIndex = 8;
        _config.OnValueChanged(CCVars.ReducedMotion, OnReducedMotionChanged, invokeImmediately: true);
    }

    public void Reset()
    {
        _intensity = 0f;
    }

    private void OnReducedMotionChanged(bool reducedMotion)
    {
        _motionScale = reducedMotion ? 0f : 1f;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        _intensity = Math.Min(1f, _intensity + args.DeltaSeconds / FadeDuration);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var player = _playerManager.LocalEntity;
        if (player == null)
            return false;

        return _entityManager.TryGetComponent<ActiveSandevistanComponent>(player.Value, out var active) &&
            GetVisualIntensity(active) > 0f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var player = _playerManager.LocalEntity;
        if (player == null ||
            !_entityManager.TryGetComponent<ActiveSandevistanComponent>(player.Value, out var active))
        {
            return;
        }

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("Intensity", GetVisualIntensity(active));
        _shader.SetParameter("MotionScale", _motionScale);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private float GetVisualIntensity(ActiveSandevistanComponent active)
    {
        var remaining = Math.Max(0f, (float) (active.EndTime - _timing.CurTime).TotalSeconds);
        var fadeOut = SmoothStep(Math.Clamp(remaining / FadeDuration, 0f, 1f));

        return Math.Min(_intensity, fadeOut);
    }

    private static float SmoothStep(float progress)
    {
        return progress * progress * (3f - 2f * progress);
    }
}
