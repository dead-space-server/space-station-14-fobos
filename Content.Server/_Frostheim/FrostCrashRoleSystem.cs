using System.Linq;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Shared._Frostheim;

namespace Content.Server._Frostheim;

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
        var spawnPoints = EntityQuery<FrostCrashSpawnPointComponent, TransformComponent>()
            .Where(x => !x.Item1.Used)
            .ToList();

        if (spawnPoints.Count == 0)
        {
            args.TookRole = false;
            return;
        }

        var (spawnComp, xform) = spawnPoints[0];
        spawnComp.Used = true;

        var mob = Spawn(spawnComp.SpawnPrototype, xform.Coordinates);
        _ghostRole.GhostRoleInternalCreateMindAndTransfer(args.Player, uid, mob, Comp<GhostRoleComponent>(uid));
        args.TookRole = true;

        QueueDel(xform.Owner);

        var remainingPoints = EntityQuery<FrostCrashSpawnPointComponent>().Count(x => !x.Used);
        if (remainingPoints == 0)
        {
            if (TryComp<GhostRoleComponent>(uid, out var ghostRole))
                ghostRole.Taken = true;

            QueueDel(uid);
        }
    }
}
