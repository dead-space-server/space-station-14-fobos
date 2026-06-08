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
}
