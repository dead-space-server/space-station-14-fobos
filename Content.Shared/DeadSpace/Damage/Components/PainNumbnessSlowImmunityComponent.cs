using Content.Shared.Damage.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Damage.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SlowOnDamageSystem))]
public sealed partial class PainNumbnessSlowImmunityComponent : Component;
