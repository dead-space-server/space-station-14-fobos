using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.HardsuitSpeedBuff;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HardsuitSpeedBuffComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionHardsuitSpeedBuff";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public float WalkModifier = 1.35f;

    [DataField, AutoNetworkedField]
    public float SprintModifier = 1.35f;

    [DataField, AutoNetworkedField]
    public bool Activated;
}
