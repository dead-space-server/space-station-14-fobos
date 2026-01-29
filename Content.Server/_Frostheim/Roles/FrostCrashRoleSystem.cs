using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Shared._Frostheim.Roles;

namespace Content.Server._Frostheim.Roles;

public sealed class FrostCrashRoleSystem : EntitySystem
{
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostCrashRoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FrostCrashRoleComponent, TakeGhostRoleEvent>(OnTakeRole);
    }

    private void OnStartup(Entity<FrostCrashRoleComponent> ent, ref ComponentStartup args)
    {
        var ghostRole = EnsureComp<GhostRoleComponent>(ent);
        ghostRole.RoleName = Loc.GetString(ent.Comp.RoleName);
        ghostRole.RoleDescription = Loc.GetString(ent.Comp.RoleDescription);
        ghostRole.RoleRules = Loc.GetString(ent.Comp.RoleRules);

        ghostRole.RaffleConfig = new GhostRoleRaffleConfig
        {
            Settings = "default",
            MinPlayers = ent.Comp.MinPlayers,
            WinnersCount = int.MaxValue
        };
    }

    private void OnTakeRole(EntityUid uid, FrostCrashRoleComponent component, ref TakeGhostRoleEvent args)
    {
        EntityUid? spawnPointUid = null;
        FrostCrashSpawnPointComponent? spawnComp = null;
        TransformComponent? xform = null;

        var query = EntityQueryEnumerator<FrostCrashSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var spUid, out var sp, out var xf))
        {
            if (sp.Used)
                continue;

            spawnPointUid = spUid;
            spawnComp = sp;
            xform = xf;
            break;
        }

        if (spawnPointUid == null || spawnComp == null || xform == null)
        {
            args.TookRole = false;
            return;
        }

        spawnComp.Used = true;

        var mob = Spawn(spawnComp.SpawnPrototype, xform.Coordinates);
        _ghostRole.GhostRoleInternalCreateMindAndTransfer(args.Player, uid, mob, Comp<GhostRoleComponent>(uid));
        args.TookRole = true;

        QueueDel(spawnPointUid.Value);

        var remainingPoints = 0;
        var countQuery = EntityQueryEnumerator<FrostCrashSpawnPointComponent>();
        while (countQuery.MoveNext(out _, out var sp2))
        {
            if (!sp2.Used)
                remainingPoints++;
        }

        if (remainingPoints == 0)
        {
            if (TryComp<GhostRoleComponent>(uid, out var ghostRole))
                ghostRole.Taken = true;

            QueueDel(uid);
        }
    }
}
