using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.GameRules;

[RegisterComponent]
public sealed partial class CashCollectionComponent : Component
{
    [DataField]
    public int Threshold = 1000000;

    [DataField]
    public float CheckInterval = 30f;

    [DataField]
    public EntProtoId CashCollectionRule = "CashCollectionRule";

    [DataField]
    public TimeSpan ShuttleCooldown = TimeSpan.FromMinutes(30);

    public TimeSpan NextAllowedSpawn = TimeSpan.Zero;

    public float CheckAccumulator = 0f;
}