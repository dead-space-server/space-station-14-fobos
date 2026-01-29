using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Frostheim.Roles;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrostCrashSpawnPointComponent : Component
{
    [DataField(required: true)]
    public EntProtoId SpawnPrototype;

    [ViewVariables]
    public bool Used;
}
