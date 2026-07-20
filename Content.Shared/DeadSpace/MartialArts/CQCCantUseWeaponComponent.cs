// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.MartialArts.CQC.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CQCCantUseWeaponComponent : Component
{
    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3.0);

    [DataField]
    [AutoPausedField]
    public TimeSpan? NextPopupTime = null;
}
