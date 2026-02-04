namespace Content.Server._Frostheim.Radar;

[RegisterComponent]
public sealed partial class ActiveFrostheimRadarComponent : Component
{
    public float AccumulatedTime = 1f; // > UpdateInterval so first update fires immediately
}
