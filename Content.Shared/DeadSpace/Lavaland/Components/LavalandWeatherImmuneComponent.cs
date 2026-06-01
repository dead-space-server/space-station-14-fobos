namespace Content.Shared.DeadSpace.Lavaland.Components;

/// <summary>
/// Prevents Lavaland weather hazards from damaging this entity.
/// Parent entities are checked too, so vehicles and sealed containers can protect their contents later.
/// </summary>
[RegisterComponent]
public sealed partial class LavalandWeatherImmuneComponent : Component
{
}
