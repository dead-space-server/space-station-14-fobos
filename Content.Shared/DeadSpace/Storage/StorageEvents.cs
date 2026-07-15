namespace Content.Shared.DeadSpace.Storage;

[ByRefEvent]
public struct CheckMultipleInventoryWindowsEvent
{
    public EntityUid Actor;

    public CheckMultipleInventoryWindowsEvent(EntityUid actor)
    {
        Actor = actor;
    }

    public bool Enabled { get; set; }
}

[ByRefEvent]
public struct GetPlayerStorageLimitEvent
{
    public EntityUid Actor;

    public GetPlayerStorageLimitEvent(EntityUid actor)
    {
        Actor = actor;
    }

    public int Limit { get; set; } = -1;
    public bool HasOverride { get; set; }
}
