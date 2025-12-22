// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;

namespace Content.Server.DeadSpace.Virus.Components;

[RegisterComponent]
public sealed partial class VirusMindComponent : Component
{
    /// <summary>
    ///     ID штамма.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public string StrainId = string.Empty;

    /// <summary>
    ///     Очки мутации, которые игрок может тратить на приобретение симптомов.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int MutationPoints = 0;

    /// <summary>
    ///     Список активных симптомов для вируса.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<VirusSymptom> ActiveSymptoms = new();
}
