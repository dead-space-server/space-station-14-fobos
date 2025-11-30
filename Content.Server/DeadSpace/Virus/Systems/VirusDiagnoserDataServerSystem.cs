// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Power.EntitySystems;
using System.Linq;
using Content.Shared.Virus;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserDataServerSystem : EntitySystem
{
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, AnchorStateChangedEvent>(OnAnchor);
        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserDataServerComponent> server, ref PortDisconnectedEvent args)
    {
        if (args.Port == server.Comp.VirusDiagnoserDataServerPort)
            server.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusDiagnoserDataServerComponent> server, ref AnchorStateChangedEvent args)
    {
        if (server.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(server.Comp.ConnectedConsole, out var console))
            return;

        if (args.Anchored)
        {
            _console.RecheckConnections((server.Comp.ConnectedConsole.Value, console));
            return;
        }

        _console.UpdateUserInterface((server.Comp.ConnectedConsole.Value, console));
    }

    public void AddPoints(Entity<VirusDiagnoserDataServerComponent?> server, int points)
    {
        if (!Resolve(server, ref server.Comp, false))
            return;

        server.Comp.Points += points;

        if (server.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(server.Comp.ConnectedConsole, out var console))
            return;

        _console.UpdateUserInterface((server.Comp.ConnectedConsole.Value, console));
    }

    public void SaveData(Entity<VirusDiagnoserDataServerComponent?> server, VirusData data)
    {
        if (!Resolve(server, ref server.Comp, false))
            return;

        if (!_powerReceiverSystem.IsPowered(server))
            return;

        var timeFormatted = _timing.CurTime.ToString(@"hh\:mm\:ss");

        var record = new VirusStrainRecord(
            data.StrainId,
            timeFormatted
        );

        server.Comp.StrainData[record] = (VirusData)data.Clone();
    }

    public void DeleteData(Entity<VirusDiagnoserDataServerComponent?> server, string strainId)
    {
        if (!Resolve(server, ref server.Comp, false))
            return;

        if (!_powerReceiverSystem.IsPowered(server))
            return;

        var key = server.Comp.StrainData.Keys
            .FirstOrDefault(k => k.Strain == strainId);

        if (!key.Equals(default(VirusStrainRecord)))
            server.Comp.StrainData.Remove(key);
    }

    public VirusData? GetData(Entity<VirusDiagnoserDataServerComponent?> server, string strainId)
    {
        if (!Resolve(server, ref server.Comp, false))
            return null;

        if (!_powerReceiverSystem.IsPowered(server))
            return null;

        var entry = server.Comp.StrainData
                    .FirstOrDefault(kvp => kvp.Key.Strain == strainId);

        // Проверка: если ключ по умолчанию — значит ничего не найдено
        if (EqualityComparer<KeyValuePair<VirusStrainRecord, VirusData>>.Default.Equals(entry, default))
            return null;

        var data = entry.Value;
        return (VirusData)data.Clone();
    }

    public List<VirusStrainRecord> GetAllStrains(Entity<VirusDiagnoserDataServerComponent?> server)
    {
        if (!Resolve(server, ref server.Comp, false))
            return new List<VirusStrainRecord>();

        return server.Comp.StrainData.Keys.ToList();
    }


}
