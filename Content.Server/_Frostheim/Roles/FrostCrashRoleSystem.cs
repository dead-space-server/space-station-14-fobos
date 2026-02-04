using Content.Server.Chat.Managers;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Server.Humanoid.Systems;
using Content.Shared._Frostheim.Roles;
using Content.Shared.Chat;
using Content.Shared.Station;
using Robust.Shared.Containers;

namespace Content.Server._Frostheim.Roles;

public sealed class FrostCrashRoleSystem : EntitySystem
{
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly RandomHumanoidSystem _randomHumanoid = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FrostCrashRoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FrostCrashRoleComponent, TakeGhostRoleEvent>(OnTakeRole);
    }

    private void OnStartup(Entity<FrostCrashRoleComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<GhostRoleComponent>(ent, out var ghostRole))
            return;

        // Raffle config from YAML doesn't deserialize fields properly,
        // so we set it programmatically. Values must match the prototype to pass PrototypeSaveTest.
        ghostRole.RaffleConfig = new GhostRoleRaffleConfig
        {
            Settings = "default",
            MinPlayers = 4,
            WinnersCount = int.MaxValue
        };
    }

    private void OnTakeRole(EntityUid uid, FrostCrashRoleComponent component, ref TakeGhostRoleEvent args)
    {
        var roleGrid = Transform(uid).GridUid;

        EntityUid? spawnPointUid = null;
        FrostCrashSpawnPointComponent? spawnComp = null;
        TransformComponent? xform = null;

        var query = EntityQueryEnumerator<FrostCrashSpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var spUid, out var sp, out var xf))
        {
            if (sp.Used)
                continue;

            if (xf.GridUid != roleGrid)
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

        var mob = _randomHumanoid.SpawnRandomHumanoid("FrostheimHumanoid", xform.Coordinates, "");

        if (!string.IsNullOrEmpty(spawnComp.ExamineLocKey))
        {
            var examine = EnsureComp<FrostCrewExamineComponent>(mob);
            examine.RoleLocKey = spawnComp.ExamineLocKey;
            examine.MessageCount = spawnComp.ExamineMessageCount;
        }

        _stationSpawning.EquipStartingGear(mob, spawnComp.StartingGear);

        EnsureComp<FrostheimCrewComponent>(mob);

        if (TryComp<FrostheimSleepPodComponent>(spawnPointUid, out var podComp) &&
            _container.TryGetContainer(spawnPointUid.Value, podComp.ContainerId, out var container))
        {
            _container.Insert(mob, container);
        }

        _ghostRole.GhostRoleInternalCreateMindAndTransfer(args.Player, uid, mob, Comp<GhostRoleComponent>(uid));
        args.TookRole = true;

        SendBriefing(args.Player.Channel);

        var remainingPoints = 0;
        var countQuery = EntityQueryEnumerator<FrostCrashSpawnPointComponent, TransformComponent>();
        while (countQuery.MoveNext(out _, out var sp2, out var xf2))
        {
            if (!sp2.Used && xf2.GridUid == roleGrid)
                remainingPoints++;
        }

        if (remainingPoints == 0)
        {
            if (TryComp<GhostRoleComponent>(uid, out var ghostRole))
                ghostRole.Taken = true;

            QueueDel(uid);
        }
    }

    private void SendBriefing(Robust.Shared.Network.INetChannel channel)
    {
        var message = Loc.GetString("frostheim-briefing");
        var wrapped = $"[font size=14][color=#8fa4b0]{message}[/color][/font]";

        _chat.ChatMessageToOne(
            ChatChannel.Server,
            message,
            wrapped,
            default,
            false,
            channel);
    }
}
