// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Power.EntitySystems;
using System.Linq;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserDataServerSystem : EntitySystem
{
    [Dependency] private readonly VirusDiagnoserConsoleSystem _console = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, AnchorStateChangedEvent>(OnAnchor);
        SubscribeLocalEvent<VirusDiagnoserDataServerComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnPortDisconnected(Entity<VirusDiagnoserDataServerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.VirusDiagnoserDataServerPort)
            ent.Comp.ConnectedConsole = null;
    }

    private void OnAnchor(Entity<VirusDiagnoserDataServerComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (ent.Comp.ConnectedConsole == null || !TryComp<VirusDiagnoserConsoleComponent>(ent.Comp.ConnectedConsole, out var console))
            return;

        if (args.Anchored)
        {
            _console.RecheckConnections((ent.Comp.ConnectedConsole.Value, console));
            return;
        }

        _console.UpdateUserInterface((ent.Comp.ConnectedConsole.Value, console));
    }

    public void SaveData(Entity<VirusDiagnoserDataServerComponent?> server, VirusData data)
    {
        if (!Resolve(server, ref server.Comp, false))
            return;

        if (!_powerReceiverSystem.IsPowered(server))
            return;

        server.Comp.StrainData[data.StrainId] = (VirusData)data.Clone();
    }


    public void DeleteData(Entity<VirusDiagnoserDataServerComponent?> server, string strainId)
    {
        if (!Resolve(server, ref server.Comp, false))
            return;

        if (!_powerReceiverSystem.IsPowered(server))
            return;

        if (!server.Comp.StrainData.ContainsKey(strainId))
            return;

        server.Comp.StrainData.Remove(strainId);
    }

    public VirusData? GetData(Entity<VirusDiagnoserDataServerComponent?> server, string strainId)
    {
        if (!Resolve(server, ref server.Comp, false))
            return null;

        if (!_powerReceiverSystem.IsPowered(server))
            return null;

        if (!server.Comp.StrainData.TryGetValue(strainId, out var data))
            return null;

        return (VirusData)data.Clone();
    }

    public List<string> GetAllStrains(Entity<VirusDiagnoserDataServerComponent?> server)
    {
        if (!Resolve(server, ref server.Comp, false))
            return new List<string>();

        return server.Comp.StrainData.Keys.ToList();
    }


}
