using Content.Shared.Access;
using Content.Shared.Stacks;
using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandOreRedeemerComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> OreValues = new();

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> MiningAccess = new()
    {
        "Salvage",
        "SeniorSalvage",
    };

    [DataField]
    public Dictionary<ProtoId<MaterialPrototype>, int> ProcessedMaterials = new();

    [DataField]
    public Dictionary<ProtoId<MaterialPrototype>, int> PendingProcessedMaterials = new();

    [DataField]
    public Dictionary<ProtoId<MaterialPrototype>, int> CreditedMaterials = new();

    [DataField]
    public Dictionary<ProtoId<MaterialPrototype>, int> PendingCreditedMaterials = new();
}
