using Content.Server.DeadSpace.MartialArts.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.Arkalyse.Components;

[RegisterComponent]
public sealed partial class ArkalyseComponent : Component
{
    [DataField]
    public TimeSpan? MuteEndTime;

    [DataField]
    public ArkalyseParams Params;

    [DataField]
    public ArkalyseList? SelectedCombo;

    public readonly List<EntProtoId> BaseArkalyse = new()
    {
        "ActionDamageArkalyseAttack",
        "ActionStunArkalyseAttack",
        "ActionMutedArkalyseAttack",
        "ActionRelaxArkalyseAttack",
    };

    public readonly List<EntityUid> ArkalyseActionEntities = new()
    {
    };

    [DataField]
    public MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.Arkalyse;
}

[RegisterComponent]
public sealed partial class ArkalyseMutedComponent : Component
{
    [ViewVariables]
    public TimeSpan Until;
}

public enum ArkalyseList
{
    DamageAttack,
    StunAttack,
    MuteAttack,
    RelaxHand,
}
