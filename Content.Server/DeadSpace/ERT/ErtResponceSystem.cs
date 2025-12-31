// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Chat.Systems;
using Content.Server.DeadSpace.Languages;
using Content.Server.GameTicking;
using Content.Shared.DeadSpace.ERT;
using Content.Shared.DeadSpace.ERT.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.ERT;

// Работает для одной станции, потому что пока нет смысла делать для множества
public sealed class ErtResponceSystem : SharedErtResponceSystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TimedWindowSystem _timedWindowSystem = default!;
    private ProtoId<ErtTeamPrototype>? _expectedTeam = null;
    private TimedWindow? _windowWaitingArrival = null;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_expectedTeam == null)
            return;

        if (_windowWaitingArrival != null && _timedWindowSystem.IsExpired(_windowWaitingArrival))
            EnsureErtTeam(_expectedTeam.Value);
    }

    public bool TryCallErt(ProtoId<ErtTeamPrototype> team)
    {
        if (_expectedTeam != null)
            return false;

        if (!_prototypeManager.TryIndex(team, out var prototype))
            return false;

        _chatSystem.DispatchGlobalAnnouncement(
            message: Loc.GetString("ert-responce-caused-messager", ("team", prototype.Name)),
            sender: Loc.GetString("chat-manager-sender-announcement"), // На всвякий
            colorOverride: Color.FromHex("#1d8bad"),
            playSound: true,
            usePresetTTS: true,
            languageId: LanguageSystem.DefaultLanguageId
        );

        _expectedTeam = team;
        _windowWaitingArrival = prototype.TimeWindowToSpawn;
        _timedWindowSystem.Reset(_windowWaitingArrival);

        return true;
    }

    public void EnsureErtTeam(ProtoId<ErtTeamPrototype> team)
    {
        if (!_prototypeManager.TryIndex(team, out var prototype))
            return;

        _expectedTeam = null;
        _windowWaitingArrival = null;

        _gameTicker.AddGameRule(prototype.ErtRule);
    }
}
