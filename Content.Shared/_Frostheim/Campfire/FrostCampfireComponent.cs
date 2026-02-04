using Robust.Shared.GameStates;

namespace Content.Shared._Frostheim.Campfire;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FrostCampfireComponent : Component
{
    [DataField, AutoNetworkedField]
    public float BurnTimeRemaining;

    [DataField]
    public float MaxBurnTime = 300f;

    [DataField]
    public float FuelPerPlank = 60f;

    [DataField, AutoNetworkedField]
    public bool Lit;
}
