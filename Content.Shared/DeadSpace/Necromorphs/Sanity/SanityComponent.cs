// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Necromorphs.Sanity;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSanitySystem))]
public sealed partial class SanityComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Ghost;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float RegenSanity = 1f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float SanityLevel = 100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float MaxSanityLevel = 100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float MinSanityLevel = -100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float UpdateDuration = 2f;

    [DataField("nextTickUtilRegen", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextTickUtilRegen = TimeSpan.Zero;

    [ViewVariables]
    public bool IsCrazy = false;

    [ViewVariables]
    public ProtoId<NpcFactionPrototype>? OldFaction = new();

    [ViewVariables]
    public string OldTask;
    [ViewVariables]
    public TimeSpan NextCheckCrazyMob = TimeSpan.Zero;
    [ViewVariables]
    public TimeSpan NextCheckPopup = TimeSpan.Zero;
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<string> LowSanityMessages = ["Братская луна поможет тебе", "Не бойся, мы всё равно станем все едины", "Смерть дарует новую жизнь", "Мы поможем тебе, просто впусти маркер истинный в себя", "МЫ. БУДЕМ. ЕДИНЫ"];
}

[ByRefEvent]
public readonly record struct SanityEvent();

[ByRefEvent]
public readonly record struct CheckCrazyMobEvent();
