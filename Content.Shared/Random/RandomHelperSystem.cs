using System.Numerics;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Random;

/// <summary>
///     System containing various content-related random helpers.
/// </summary>
public sealed class RandomHelperSystem : EntitySystem
{
    // DS14-start: current engine uses readonly IoC fields.
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    // DS14-end

    public void RandomOffset(EntityUid entity, float minX, float maxX, float minY, float maxY, IRobustRandom? random = null)
    {
        random ??= SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(entity));

        var randomX = random.NextFloat() * (maxX - minX) + minX;
        var randomY = random.NextFloat() * (maxY - minY) + minY;
        var offset = new Vector2(randomX, randomY);

        var xform = Transform(entity);
        _transform.SetLocalPosition(entity, xform.LocalPosition + offset, xform);
    }

    public void RandomOffset(EntityUid entity, float min, float max, IRobustRandom? random = null)
    {
        RandomOffset(entity, min, max, min, max, random);
    }

    public void RandomOffset(EntityUid entity, float value, IRobustRandom? random = null)
    {
        RandomOffset(entity, -value, value, random);
    }
}
