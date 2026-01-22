using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Events
{
    [Serializable, NetSerializable]
    public sealed class ServerReloadRequestEvent : EntityEventArgs
    {
        public bool ReloadNow { get; set; } = false;

        public ServerReloadRequestEvent()
        {
        }

        public ServerReloadRequestEvent(bool reloadNow)
        {
            ReloadNow = reloadNow;
        }
    }
}
