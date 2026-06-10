using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.NicotineAddiction;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class NicotineAddictionComponent : Component
{
    [DataField]
    public float RequiredNicotineLevel = 0.0001f;

    [DataField]
    public TimeSpan DeprivationPopupDelay = TimeSpan.FromMinutes(5);

    [DataField]
    public TimeSpan PopupToShakeDelay = TimeSpan.FromMinutes(2);

    public TimeSpan LastNicotineInBloodTime = TimeSpan.Zero;
    public bool DeprivationPopupShown;
    public TimeSpan DeprivationPopupShownAt = TimeSpan.Zero;

    [AutoNetworkedField]
    public bool DeprivationShakeActive;
}
