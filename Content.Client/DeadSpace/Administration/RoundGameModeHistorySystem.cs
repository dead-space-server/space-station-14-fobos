// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Administration.Events;

namespace Content.Client.DeadSpace.Administration;

public sealed class RoundGameModeHistorySystem : EntitySystem
{
    public RoundGameModeHistoryEntry[]? Entries { get; private set; }

    public event Action? HistoryUpdated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RoundGameModeHistoryResponseEvent>(OnHistoryResponse);
    }

    public void RequestHistory()
    {
        Entries = null;
        HistoryUpdated?.Invoke();
        RaiseNetworkEvent(new RoundGameModeHistoryRequestEvent());
    }

    private void OnHistoryResponse(RoundGameModeHistoryResponseEvent msg, EntitySessionEventArgs args)
    {
        Entries = msg.Entries;
        HistoryUpdated?.Invoke();
    }
}
