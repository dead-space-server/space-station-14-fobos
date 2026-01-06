using Content.Shared.DeviceLinking.Events;
using Content.Server.Research.Components;
using Content.Server.Research.TechnologyDisk.Components;
using Content.Server.Research.Systems;
using Robust.Server.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Research.Systems;

public sealed class TechDiskPrinterOnSignalSystem : EntitySystem
{
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

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

        // Уже печатается
        if (HasComp<DiskConsolePrintingComponent>(uid))
            return;

        // Проверяем наличие исследовательского сервера и очков
        if (!_research.TryGetClientServer(uid, out var server, out var serverComp))
            return;

        if (!TryComp<DiskConsoleComponent>(uid, out var console))
            return;

        if (serverComp.Points < console.PricePerDisk)
            return;

        // Списываем очки
        _research.ModifyServerPoints(server.Value, -console.PricePerDisk, serverComp);

        // Звук печати
        _audio.PlayPvs(console.PrintSound, uid);

        // Запускаем печать
        var printing = EnsureComp<DiskConsolePrintingComponent>(uid);
        printing.FinishTime = _timing.CurTime + console.PrintDuration;
    }
}
