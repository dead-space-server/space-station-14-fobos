using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Arena;

[Serializable, NetSerializable]
public sealed class ArenaJoinEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaLeaveEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ArenaOfferEvent : EntityEventArgs
{
    public List<ArenaOfferEntry> Catalog { get; }

    public ArenaOfferEvent(List<ArenaOfferEntry> catalog)
    {
        Catalog = catalog;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaOfferEntry
{
    public int Idx;
    public string Title = string.Empty;
    public string Hint = string.Empty;
    public string Style = string.Empty;
    public string Icon = string.Empty;
}

[Serializable, NetSerializable]
public sealed class ArenaPickEvent : EntityEventArgs
{
    public int Pick { get; }

    public ArenaPickEvent(int pick)
    {
        Pick = pick;
    }
}
