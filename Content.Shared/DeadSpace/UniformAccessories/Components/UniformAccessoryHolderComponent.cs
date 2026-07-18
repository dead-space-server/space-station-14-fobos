using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.UniformAccessories.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState(true)]
[Access(typeof(SharedUniformAccessorySystem))]
public sealed partial class UniformAccessoryHolderComponent : Component
{
    /// <summary>
    /// The container used to store accessories on this holder.
    /// </summary>
    public const string ContainerId = "uniform_accessories";

    /// <summary>
    /// The container for storing accessories.
    /// </summary>
    [ViewVariables]
    public Container? AccessoryContainer;

    /// <summary>
    /// Categories of accessories allowed on this holder.
    /// </summary>
    [DataField] [AutoNetworkedField]
    public List<string> AllowedCategories = new();

    /// <summary>
    /// Cached accessory names used by examine when contained entities are outside the observing client's PVS.
    /// </summary>
    [AutoNetworkedField]
    public List<string> ExamineNames = new();

    /// <summary>
    /// Cached accessory colors corresponding to <see cref="ExamineNames"/>.
    /// </summary>
    [AutoNetworkedField]
    public List<string> ExamineColors = new();
}
