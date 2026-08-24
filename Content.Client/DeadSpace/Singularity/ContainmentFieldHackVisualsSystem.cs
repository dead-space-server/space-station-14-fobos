// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Shared.Singularity.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.DeadSpace.Singularity;

public sealed class ContainmentFieldHackVisualsSystem : EntitySystem
{
    private static readonly ProtoId<ShaderPrototype> HackShader = "ContainmentFieldHack";

    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<EntityUid, ShaderInstance> _shaders = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainmentFieldGeneratorComponent, ComponentShutdown>(OnGeneratorShutdown);
        SubscribeLocalEvent<ContainmentFieldComponent, ComponentShutdown>(OnFieldShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var generators = EntityQueryEnumerator<ContainmentFieldGeneratorComponent, SpriteComponent>();
        while (generators.MoveNext(out var uid, out var generator, out var sprite))
        {
            if (generator.HackEndTime is not { } endTime)
                continue;

            if (!_shaders.TryGetValue(uid, out var shader))
            {
                shader = _prototypes.Index(HackShader).InstanceUnique();
                _shaders.Add(uid, shader);
                for (var i = 0; i < sprite.AllLayers.Count(); i++)
                    sprite.LayerSetShader(i, shader, HackShader.Id);
            }

            shader.SetParameter("progress", Math.Clamp(
                1f - (float) (endTime - _timing.CurTime).TotalSeconds / ContainmentFieldGeneratorComponent.HackDurationSeconds,
                0f,
                1f));
        }

        var fields = EntityQueryEnumerator<ContainmentFieldComponent, SpriteComponent>();
        while (fields.MoveNext(out var uid, out var field, out var sprite))
        {
            if (field.HackEndTime is not { } endTime)
                continue;

            if (!_shaders.TryGetValue(uid, out var shader))
            {
                shader = _prototypes.Index(HackShader).InstanceUnique();
                _shaders.Add(uid, shader);
                sprite.LayerSetShader(0, shader, HackShader.Id);
            }

            shader.SetParameter("progress", field.HackIntensity * Math.Clamp(
                1f - (float) (endTime - _timing.CurTime).TotalSeconds / ContainmentFieldGeneratorComponent.HackDurationSeconds,
                0f,
                1f));
        }
    }

    private void OnGeneratorShutdown(Entity<ContainmentFieldGeneratorComponent> generator, ref ComponentShutdown args)
    {
        if (_shaders.Remove(generator, out var shader))
            shader.Dispose();
    }

    private void OnFieldShutdown(Entity<ContainmentFieldComponent> field, ref ComponentShutdown args)
    {
        if (_shaders.Remove(field, out var shader))
            shader.Dispose();
    }
}
