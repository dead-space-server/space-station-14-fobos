using Content.Shared.DeadSpace.Arena;
using Content.Shared.DeadSpace.Arena.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.DeadSpace.Arena;

[UsedImplicitly]
public sealed class ArenaSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ArenaFlagComponent, ComponentInit>(OnFlagInit);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ArenaFlagComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var flag, out var sprite))
        {
            var expected = flag.Team switch
            {
                ArenaTeam.Blue => Color.FromHex("#4488FF"),
                ArenaTeam.Red => Color.FromHex("#FF4444"),
                _ => Color.FromHex("#FFD700")
            };

            if (sprite.Color != expected)
                sprite.Color = expected;
        }
    }

    private void OnFlagInit(Entity<ArenaFlagComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.Color = ent.Comp.Team switch
        {
            ArenaTeam.Blue => Color.FromHex("#4488FF"),
            ArenaTeam.Red => Color.FromHex("#FF4444"),
            _ => Color.FromHex("#FFD700")
        };
    }
}
