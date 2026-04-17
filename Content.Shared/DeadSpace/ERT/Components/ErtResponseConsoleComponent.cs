// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Cargo.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeadSpace.ERT.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.ERT.Components;

[RegisterComponent, ComponentProtoName("ErtResponseConsole"), NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ErtResponseConsoleComponent : Component
{
    public const string AuthSlotAId = "ErtConsole-auth-a";
    public const string AuthSlotBId = "ErtConsole-auth-b";

    [DataField]
    public List<ProtoId<ErtTeamPrototype>> Teams = new();

    [DataField]
    public ProtoId<CargoAccountPrototype> Account = "Security";

    [DataField]
    public bool UseApprovalWorkflow = true;

    [DataField, AutoNetworkedField]
    public bool RequireDualAuthorization = true;

    [AutoNetworkedField]
    public bool IsAuthorized;

    [DataField]
    public ItemSlot AuthSlotA = new()
    {
        Name = "ert-console-auth-slot-a",
        Swap = false,
    };

    [DataField]
    public ItemSlot AuthSlotB = new()
    {
        Name = "ert-console-auth-slot-b",
        Swap = false,
    };
}
