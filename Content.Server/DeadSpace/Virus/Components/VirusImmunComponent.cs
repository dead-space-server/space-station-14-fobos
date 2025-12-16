// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusImmunComponent : Component
{
    /// <summary>
    ///     Штаммы к которым у сущности есть иммунитет.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public List<string> StrainsId = new();

    [DataField]
    public bool ImmunAll = false;
}

