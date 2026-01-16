// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
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
