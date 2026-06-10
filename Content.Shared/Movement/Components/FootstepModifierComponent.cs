using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Changes footstep sound
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootstepModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public SoundSpecifier? FootstepSoundCollection;

    //DS14-start
    [DataField, AutoNetworkedField]
    public HashSet<string>? AllowedSoundCollections;
    //DS14-end
}
