using Content.Server.DeadSpace.MartialArts;
using Content.Server.DeadSpace.MartialArts.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.MartialArts.Arkalyse.Component;

[RegisterComponent]
public sealed partial class ArkalyseComponent : Component
{
    [DataField]
    public ArkalyseParams Params { get; set; }

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

public enum ArkalyseList
{
    DamageAtack,
    StunAtack,
    MuteAtack,
    RelaxHand,
}
