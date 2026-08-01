using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.QueueTerminal;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class QueueTicketComponent : Component
{

    [DataField, AutoNetworkedField]
    public int Number;

    [DataField]
    public EntityUid? Terminal;

    public EntityUid? Owner;
}
