// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Abilities.Slide;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpeedSlidingComponent : Component
{
    [DataField] 
    public float MinSlideSpeed = 4.9f;

    [DataField] 
    public float SlideDistance = 8.5f;

    [DataField] 
    public float SlideSpeed = 3.5f;

    [DataField] 
    public SoundSpecifier? SlideSound;

    [ViewVariables]
    public bool IsSliding = false;
}