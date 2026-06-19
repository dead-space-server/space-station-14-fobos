// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Autopsy;

/// <summary>
///     On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class AutopsyScannerScannedUserMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;
    public TimeSpan? TimeOfDeath;
    public string? EntityName;
    public FixedPoint2 TotalDamage;
    public List<DamageGroupEntry> DamagePerGroup = new();
    public List<DamageEventEntry> DamageSequence = new();

    public AutopsyScannerScannedUserMessage(NetEntity? targetEntity, TimeSpan? timeOfDeath)
    {
        TargetEntity = targetEntity;
        TimeOfDeath = timeOfDeath;
    }
}

[Serializable, NetSerializable]
public sealed class DamageGroupEntry
{
    public string DamageGroupId = string.Empty;
    public FixedPoint2 Amount;
}

[Serializable, NetSerializable]
public sealed class DamageEventEntry
{
    public TimeSpan TimeOfDamageTake;
    public string DamageGroup = string.Empty;
    public string DamageType = string.Empty;
    public FixedPoint2 DamageTaken;
}
