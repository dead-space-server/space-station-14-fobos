// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.GameTicking.Components;
using Content.Server.GameTicking.Rules.Components;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
using Content.Shared.Fax.Components;
using Content.Shared.Paper;
using Content.Server.Fax;

namespace Content.Server.GameTicking.Rules;

public sealed class NecroobeliskArtefactRuleSystem : GameRuleSystem<NecroobeliskArtefactRuleComponent>
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly FaxSystem _faxSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Started(EntityUid uid, NecroobeliskArtefactRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.TimeUntilStart = _timing.CurTime + component.StartDuration;
    }

    protected override void ActiveTick(EntityUid uid, NecroobeliskArtefactRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (_timing.CurTime >= component.TimeUntilStart && !component.IsArtefactSended)
            StartRule(uid);

        return;
    }

    private void StartRule(EntityUid uid, NecroobeliskArtefactRuleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("uni-centcomm-announcement-send-obelisk-artefact"), playSound: true, colorOverride: Color.Green);
        GameTicker.StartGameRule("GiftsNecroobeliskArtefact");
        component.IsArtefactSended = true;

        var faxes = EntityQueryEnumerator<FaxMachineComponent>();

        while (faxes.MoveNext(out var faxEnt, out var fax))
        {
            if (!fax.ReceiveNukeCodes)
                continue;

            var content = Loc.GetString("paper-order-obelisk");

            var printout = new FaxPrintout(
                content,
                Loc.GetString("paper-order-name"),
                null,
                prototypeId: "PaperPrintedCentcomm",
                "paper_stamp-centcom",
                new List<StampDisplayInfo>
                {
                    new StampDisplayInfo { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
                },
                signatures: new List<string> { "Эвелин Маршалл" }
            );

            _faxSystem.Receive(faxEnt, printout, null, fax);
        }
    }
}
