// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Shared.DeadSpace.Flashbang;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FlashbangComponent : BaseXOnTriggerComponent
{
    [DataField]
    public float KnockdownRange = 6f;

    [DataField]
    public float StunRange = 4f;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public float Probability = 1f;
}
