using Content.Shared.Examine;

namespace Content.Shared.DeadSpace.FakeDescription;

public sealed class FakeClothingSlowOnDamageModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FakeClothingSlowOnDamageModifierComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<FakeClothingSlowOnDamageModifierComponent> ent, ref ExaminedEvent args)
    {
        var msg = Loc.GetString("slow-on-damage-modifier-examine", ("mod", (1 - ent.Comp.Modifier) * 100));
        args.PushMarkup(msg);
    }
}
