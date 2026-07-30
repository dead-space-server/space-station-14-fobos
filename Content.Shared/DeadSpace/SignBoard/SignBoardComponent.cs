// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.SignBoard;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SignBoardComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Text = string.Empty;

    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("PaperScribbles");

    [DataField]
    public int MaxLength = 12;
}
