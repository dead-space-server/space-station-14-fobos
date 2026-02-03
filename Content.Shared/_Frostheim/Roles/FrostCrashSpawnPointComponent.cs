using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrostCrashSpawnPointComponent : Component
{
    [DataField]
    public ProtoId<StartingGearPrototype>? StartingGear;

    [DataField]
    public string? ExamineLocKey;

    [DataField]
    public int ExamineMessageCount = 3;

    [ViewVariables]
    public bool Used;
}
