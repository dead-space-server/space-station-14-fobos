using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.DeadSpace.NicotineAddiction;
using Robust.Client.Player;
using Robust.Shared.Random;

namespace Content.Client.DeadSpace.NicotineAddiction;

public sealed class NicotineDeprivationEffectsSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _cameraRecoil = default!;

    private const float ScreenKick = 0.12f;
    private const float EyeNudge = 0.04f;
    private Vector2 _eyeNudge;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NicotineAddictionComponent, GetEyeOffsetEvent>(OnEyeOffset);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var local = _player.LocalEntity;
        if (local == null || !TryComp<NicotineAddictionComponent>(local, out var c) || !c.DeprivationShakeActive)
        {
            _eyeNudge = Vector2.Zero;
            return;
        }

        _cameraRecoil.KickCamera(local.Value,
            new Vector2(_random.NextFloat(-1f, 1f), _random.NextFloat(-1f, 1f)) * ScreenKick);

        var t = new Vector2(_random.NextFloat(-1f, 1f), _random.NextFloat(-1f, 1f)) * EyeNudge;
        _eyeNudge = Vector2.Lerp(_eyeNudge, t, 0.35f);
    }

    private void OnEyeOffset(EntityUid uid, NicotineAddictionComponent comp, ref GetEyeOffsetEvent args)
    {
        if (!comp.DeprivationShakeActive || uid != _player.LocalEntity)
            return;
        args.Offset += _eyeNudge;
    }
}
