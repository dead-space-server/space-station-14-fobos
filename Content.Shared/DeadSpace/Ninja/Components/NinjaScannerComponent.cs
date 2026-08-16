using Robust.Shared.GameStates;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Ninja.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjaScannerComponent : Component
{
    [DataField]
    public EntProtoId ScanAction = "ActionNinjaScan";

    [DataField, AutoNetworkedField]
    public EntityUid? ScanActionEntity;

    [DataField, AutoNetworkedField]
    public List<NinjaScanData> ScannedTargets = new();

    [DataField]
    public int MaxScans = 3;

    [DataField]
    public EntProtoId OpenUiAction = "ActionNinjaOpenScanner";

    [DataField, AutoNetworkedField]
    public EntityUid? OpenUiActionEntity;
}

public sealed partial class NinjaScanActionEvent : EntityTargetActionEvent;

public sealed partial class NinjaOpenScannerActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class NinjaScanData
{
    public string Name;
    public NetEntity Target;

    public NinjaScanData(string name, NetEntity target)
    {
        Name = name;
        Target = target;
    }
}

[Serializable, NetSerializable]
public enum NinjaScannerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class NinjaScannerBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<NinjaScanData> Targets;

    public NinjaScannerBoundUserInterfaceState(List<NinjaScanData> targets)
    {
        Targets = targets;
    }
}

[Serializable, NetSerializable]
public sealed class NinjaApplyDisguiseMessage : BoundUserInterfaceMessage
{
    public NetEntity Target;

    public NinjaApplyDisguiseMessage(NetEntity target)
    {
        Target = target;
    }
}