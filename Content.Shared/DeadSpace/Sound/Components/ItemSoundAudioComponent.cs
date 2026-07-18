// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Sound.Components;

/// <summary>
/// Marks audio entities that belong to generic item interaction sounds.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class ItemSoundAudioComponent : Component
{
    /// <summary>
    /// Original audio volume before the local item sound volume slider is applied.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BaseVolume;
}
