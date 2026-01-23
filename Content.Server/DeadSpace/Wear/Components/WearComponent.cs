// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Wear.Components;

[RegisterComponent]
public sealed partial class WearComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? Sound = default!;
}