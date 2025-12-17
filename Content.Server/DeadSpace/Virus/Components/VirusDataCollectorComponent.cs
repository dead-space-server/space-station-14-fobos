// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusDataCollectorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public string DNA = String.Empty;

    /// <summary>
    ///     Длительность сбора материала.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float Duration = 3f;

    /// <summary>
    ///     Требуемое расстояние до цели.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float Distance = 1f;

    /// <summary>
    ///     Использован ли?
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsUsed = false;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public VirusData? Data = null;
}
