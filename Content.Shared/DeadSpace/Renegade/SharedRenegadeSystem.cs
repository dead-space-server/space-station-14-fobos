// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.DeadSpace.Renegade;

public abstract class SharedRenegadeSystem : EntitySystem
{
}

public sealed partial class RenegadeSubmissionEvent : EntityTargetActionEvent { }

public sealed partial class RenegadeLightningEvent : EntityTargetActionEvent { }

public sealed partial class RenegadeForceOneEvent : EntityTargetActionEvent { }

public sealed partial class RenegadeForceEvent : InstantActionEvent { }

public sealed partial class RenegadeShieldEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class RenegadeSubmissionDoAfterEvent : SimpleDoAfterEvent { }
