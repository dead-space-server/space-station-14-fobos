// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.CrewManifest;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.CrewManifest;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Paper;
using Content.Shared.PDA;
using Robust.Server.GameObjects;

namespace Content.Server.Paper;

public sealed class PaperInsertDataSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly CrewManifestSystem _crewManifest = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaperComponent, PaperInsertDataRequestMessage>(OnRequest);
    }

    private void OnRequest(Entity<PaperComponent> ent, ref PaperInsertDataRequestMessage msg)
    {
        if (!Equals(msg.UiKey, PaperComponent.PaperUiKey.Write))
            return;

        var actor = msg.Actor;

        var station = _station.GetOwningStation(actor);
        var stationName = station is { } stationId ? MetaData(stationId).EntityName : null;

        var roundDateTime = $"{_gameTicker.RoundDuration():hh\\:mm\\:ss} {DateTime.UtcNow.AddHours(3):dd.MM}.2710";

        var characterName = MetaData(actor).EntityName;

        string? characterJob = null;
        if (_idCard.TryFindIdCard(actor, out var idCard))
            characterJob = idCard.Comp.LocalizedJobTitle;

        var manifest = new List<CrewManifestEntry>();
        if (station is { } manifestStation && TryFindPda(actor, out _))
        {
            var (_, entries) = _crewManifest.GetCrewManifest(manifestStation);
            if (entries != null)
                manifest.AddRange(entries.Entries);
        }

        manifest.Sort((a, b) => string.Compare(a.JobTitle, b.JobTitle, StringComparison.CurrentCultureIgnoreCase));

        var response = new PaperInsertDataResponseMessage(stationName, roundDateTime, characterName, characterJob, manifest);
        _ui.ServerSendUiMessage(ent.Owner, msg.UiKey, response, actor);
    }
    private bool TryFindPda(EntityUid uid, out EntityUid pda)
    {
        if (_hands.GetActiveItem(uid) is { } heldItem && HasComp<PdaComponent>(heldItem))
        {
            pda = heldItem;
            return true;
        }

        if (_inventory.TryGetSlotEntity(uid, "id", out var idSlotUid) && HasComp<PdaComponent>(idSlotUid!.Value))
        {
            pda = idSlotUid.Value;
            return true;
        }

        pda = default;
        return false;
    }
}
