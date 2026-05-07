using Content.Shared.Shuttles.Systems;
using Content.Shared.DeadSpace.Shuttles.Components; //DS14
using Robust.Shared.Prototypes; //DS14
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedRadarConsoleSystem))]
public sealed partial class RadarConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float RangeVV
    {
        get => MaxRange;
        set => IoCManager
            .Resolve<IEntitySystemManager>()
            .GetEntitySystem<SharedRadarConsoleSystem>()
            .SetRange(Owner, value, this);
    }

    [DataField, AutoNetworkedField]
    public float MaxRange = 256f;

    /// <summary>
    /// If true, the radar will be centered on the entity. If not - on the grid on which it is located.
    /// </summary>
    [DataField]
    public bool FollowEntity = false;

    //DS14-start
    [DataField]
    public bool Advanced = false;

    [DataField]
    public List<RadarBlipEntry> AllowedComponents = new();

    [DataField]
    public List<string> BlacklistComponents = new();

    [DataField]
    public List<RadarBlipTagEntry> AllowedTags = new();

    [DataField]
    public List<string> BlacklistTags = new();

    [DataField]
    public EntProtoId? ToggleAction;

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
    //DS14-end
}
