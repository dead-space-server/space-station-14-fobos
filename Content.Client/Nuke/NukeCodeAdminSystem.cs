using Content.Shared.DeadSpace.ERT;

namespace Content.Client.Nuke;

public sealed class NukeCodeAdminSystem : EntitySystem
{
    public NukeCodesAdminStateResponse? LastState { get; private set; }

    public event Action? OnStateUpdated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<NukeCodesAdminStateResponse>(OnNukeCodesAdminStateResponse);
        SubscribeNetworkEvent<NukeCodesAdminActionResult>(OnNukeCodesAdminActionResult);
    }

    private void OnNukeCodesAdminStateResponse(NukeCodesAdminStateResponse msg, EntitySessionEventArgs args)
    {
        LastState = msg;
        OnStateUpdated?.Invoke();
    }

    private void OnNukeCodesAdminActionResult(NukeCodesAdminActionResult msg, EntitySessionEventArgs args)
    {
        Log.Warning(msg.Message);
    }

    public void RequestAdminState()
    {
        RaiseNetworkEvent(new RequestNukeCodesAdminStateMessage());
    }

    public void QueueNukeCodes(NetEntity station, string reasonProtoId)
    {
        RaiseNetworkEvent(new AdminQueueNukeCodesMessage(station, reasonProtoId));
    }

    public void ApproveNukeCodes(int requestId)
    {
        RaiseNetworkEvent(new AdminApproveNukeCodesMessage(requestId));
    }

    public void CancelNukeCodes(int requestId)
    {
        RaiseNetworkEvent(new AdminCancelNukeCodesMessage(requestId));
    }
}
