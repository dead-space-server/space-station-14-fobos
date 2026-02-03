using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Random;

namespace Content.Shared._Frostheim.Roles;

public sealed class FrostCrewExamineSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrostCrewExamineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, FrostCrewExamineComponent component, ExaminedEvent args)
    {
        if (string.IsNullOrEmpty(component.RoleLocKey) || component.MessageCount <= 0)
            return;

        if (args.Examiner == uid)
        {
            var selfKey = $"{component.RoleLocKey}-self";
            args.PushMarkup(Loc.GetString(selfKey));
            return;
        }

        var identity = Identity.Entity(uid, EntityManager);
        var index = _random.Next(1, component.MessageCount + 1);
        var key = $"{component.RoleLocKey}-{index}";

        args.PushMarkup(Loc.GetString(key, ("target", identity)));
    }
}
