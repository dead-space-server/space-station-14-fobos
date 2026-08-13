// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.SignatureOnPaper.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SignaturePaperComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public int NumberSignatures = 0;

    [DataField, AutoNetworkedField]
    public int MaximumSignatures = 10;
}
