// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The duration, in seconds, of the post-round automatic map vote.
    /// </summary>
    public static readonly CVarDef<int> VoteAutoMapDuration =
        CVarDef.Create("vote.auto_map.duration", 90, CVar.ARCHIVE | CVar.SERVERONLY);
}
