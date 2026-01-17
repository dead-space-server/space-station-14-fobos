using Content.Shared.DeviceLinking.Events;
using Content.Server.Research.Components;
using Content.Server.Research.TechnologyDisk.Components;
using Content.Shared.Research; // DiskConsolePrintDiskMessage
using Robust.Shared.GameObjects;

namespace Content.Server.Research.Systems;

public sealed class TechDiskPrinterOnSignalSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<TechDiskPrinterOnSignalComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(
        EntityUid uid,
        TechDiskPrinterOnSignalComponent component,
        ref SignalReceivedEvent args)
    {
        if (args.Port != component.PrintPort)
            return;

        // Создаем сообщение без актора
        var message = new DiskConsolePrintDiskMessage();
        RaiseLocalEvent(uid, message);
    }
}
