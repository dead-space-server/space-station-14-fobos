using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.WaterCooler;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToggleableSolutionTransferComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string Solution = "tank";

    [DataField, AutoNetworkedField]
    public SolutionTransferDirection Direction = SolutionTransferDirection.Output;
}

public enum SolutionTransferDirection
{
    Input,
    Output,
}
