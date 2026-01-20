// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.TimeWindow;

namespace Content.Server.DeadSpace.Necromorphs.Unitology;


[RegisterComponent, Access(typeof(CircleOpsRuleSystem))]
public sealed partial class CircleOpsRuleComponent : Component
{
    /// <summary>
    ///    Окно времени до появления возможности обьявить войну
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow WindowBefoteWarDeclaration = new(TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20));

    /// <summary>
    ///    Окно времени до появления возможности прилететь на станцию после обьявления войны
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow WindowAfterWarDeclare = new(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));

    /// <summary>
    ///    Окно времени до спавна обелиска на станцию
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow WindowUntilSendObelisk = new(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    /// <summary>
    ///    Окно времени до появления луны
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimedWindow WindowUntilSpawnMoon = new(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? TargetStation;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Shuttle;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Obelisk;

    [ViewVariables(VVAccess.ReadOnly)]
    public CircleOpsState State = CircleOpsState.Inactive;
}

public enum CircleOpsState : byte
{
    Inactive,
    Preparing,
    WarReady,
    WarDeclared,
    ObeliskActivated,
    Convergence
}