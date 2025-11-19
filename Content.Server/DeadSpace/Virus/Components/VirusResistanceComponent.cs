// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Virus.Components;

/// <summary>
///     Логика резистов зомби инфекции.
/// </summary>
[RegisterComponent]
public sealed partial class VirusResistanceComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float VirusResistanceCoefficient = 0.1f;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public LocId Examine = "virus-resistance-coefficient-value";
}
