// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Content.Shared.DeadSpace.Weapons.Parry;
using Content.Shared.Hands.EntitySystems;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Client.DeadSpace.Weapons.Parry;

public sealed class ParryCooldownOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private readonly IEntityManager _entities;
    private readonly IGameTiming _timing;
    private readonly IInputManager _input;
    private readonly IPlayerManager _players;
    private readonly SharedHandsSystem _hands;

    public ParryCooldownOverlay(
        IEntityManager entities,
        IGameTiming timing,
        IInputManager input,
        IPlayerManager players,
        SharedHandsSystem hands)
    {
        _entities = entities;
        _timing = timing;
        _input = input;
        _players = players;
        _hands = hands;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_players.LocalEntity is not { } user ||
            !_hands.TryGetActiveItem(user, out var held) ||
            !_entities.TryGetComponent<ParryComponent>(held, out var parry) ||
            parry.NextParry <= _timing.CurTime)
        {
            return;
        }

        var active = parry.ActiveUntil > _timing.CurTime;
        var phaseStart = active || parry.ActiveUntil > parry.CooldownStart
            ? parry.ActiveUntil
            : parry.CooldownStart;
        var phaseEnd = active ? parry.ActiveUntil : parry.NextParry;
        if (active)
            phaseStart = parry.CooldownStart;

        var duration = (phaseEnd - phaseStart).TotalSeconds;
        if (duration <= 0)
            return;

        var remaining = Math.Clamp(
            (phaseEnd - _timing.CurTime).TotalSeconds / duration,
            0d,
            1d);

        var handle = args.ScreenHandle;
        var center = _input.MouseScreenPosition.Position;
        var time = (float) _timing.CurTime.TotalSeconds;
        var pulse = 0.82f + MathF.Sin(time * (active ? 10f : 4f)) * 0.12f;
        var color = active
            ? Color.FromHex("#7BEAFFFF")
            : Color.FromHex("#FFB75EFF");

        DrawArcRing(handle, center, 32f, 42f, 0f, MathF.Tau, Color.Black.WithAlpha(0.38f));
        DrawSegmentedRing(handle, center, 34f, 40f, (float) remaining, color.WithAlpha(0.82f * pulse));
        DrawArcRing(handle, center, 42f, 43.5f, 0f, MathF.Tau, color.WithAlpha(0.16f * pulse));

        var glintAngle = -MathF.PI / 2f + time * (active ? 3.5f : 1.4f);
        DrawArcRing(handle, center, 41f, 45f, glintAngle, glintAngle + 0.22f, color.WithAlpha(0.75f));

        handle.DrawCircle(center, 29f, color.WithAlpha(active ? 0.07f * pulse : 0.035f));
    }

    private static void DrawSegmentedRing(
        DrawingHandleScreen handle,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float remaining,
        Color color)
    {
        const int segments = 16;
        const float gap = 0.045f;
        var completed = remaining * segments;

        for (var i = 0; i < segments; i++)
        {
            var fill = Math.Clamp(completed - i, 0f, 1f);
            if (fill <= 0f)
                break;

            var start = -MathF.PI / 2f + MathF.Tau * i / segments + gap;
            var end = -MathF.PI / 2f + MathF.Tau * (i + fill) / segments - gap;
            DrawArcRing(handle, center, innerRadius, outerRadius, start, end, color);
        }
    }

    private static void DrawArcRing(
        DrawingHandleScreen handle,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color color)
    {
        const int resolution = 48;
        var sweep = endAngle - startAngle;
        var segmentCount = Math.Max(1, (int) MathF.Ceiling(resolution * MathF.Abs(sweep) / MathF.Tau));
        var vertices = new Vector2[(segmentCount + 1) * 2];

        for (var i = 0; i <= segmentCount; i++)
        {
            var angle = startAngle + sweep * i / segmentCount;
            var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            vertices[i * 2] = center + direction * outerRadius;
            vertices[i * 2 + 1] = center + direction * innerRadius;
        }

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleStrip, vertices, color);
    }
}
