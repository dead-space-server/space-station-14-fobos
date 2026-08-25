using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.BSAConsole;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BluespaceArtilleryComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsReady = true;

    [DataField, AutoNetworkedField]
    public float CooldownEnd;

    [DataField]
    public float CooldownDuration = 60f;
}
