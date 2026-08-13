using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Starlight.Antags.Abductor.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AbductorVictimComponent : Component
{
    [DataField("position"), AutoNetworkedField]
    public EntityCoordinates? Position;

    [DataField("organ"), AutoNetworkedField]
    public AbductorOrganType Organ = AbductorOrganType.None;

    [DataField]
    public TimeSpan? LastActivation;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AbductorOrganComponent : Component
{
    [DataField("organ"), AutoNetworkedField]
    public AbductorOrganType Organ;
}
