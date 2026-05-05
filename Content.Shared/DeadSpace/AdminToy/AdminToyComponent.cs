// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.Eui;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AdminToy;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class AdminToyComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ToyPrototype = string.Empty;

    [DataField]
    public EntProtoId BooAction = "ActionAdminToyBoo";

    [DataField]
    public EntProtoId LightningAction = "ActionAdminToyLightning";

    [DataField]
    public string LightningPrototype = "LightningRevenant";

    [DataField]
    public float ConstructionRange = 20f;

    [DataField]
    public float BooRadius = 3f;

    [DataField]
    public int BooMaxTargets = 3;

    [DataField, AutoNetworkedField]
    public EntityUid? BooActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? LightningActionEntity;

    [ViewVariables]
    public NetUserId? AdminUserId;

    [ViewVariables]
    public NetUserId? TargetUserId;

    [ViewVariables]
    public EntityUid? TargetEntity;

    [ViewVariables]
    public EntityUid? AdminMindId;

    [ViewVariables]
    public ushort PrivateVisibilityLayer;

    [ViewVariables]
    public readonly HashSet<int> ConstructionGhosts = new();

    [ViewVariables]
    public bool CleaningUp;

    [ViewVariables]
    public string? AppliedToyPrototype;
}

[RegisterComponent]
public sealed partial class AdminToySpawnableComponent : Component;

public sealed partial class AdminToyBooActionEvent : InstantActionEvent;

public sealed partial class AdminToyLightningActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed class AdminToySelectionEuiState : EuiStateBase
{
    public readonly NetEntity Target;

    public AdminToySelectionEuiState(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class AdminToySelectedMessage : EuiMessageBase
{
    public readonly string Prototype;
    public readonly string Name;
    public readonly string Description;
    public readonly string TtsVoice;

    public AdminToySelectedMessage(string prototype, string name, string description, string ttsVoice)
    {
        Prototype = prototype;
        Name = name;
        Description = description;
        TtsVoice = ttsVoice;
    }
}

[Serializable, NetSerializable]
public sealed class AdminToyPlaceConstructionGhostRequest : EntityEventArgs
{
    public readonly NetCoordinates Coordinates;
    public readonly string Prototype;
    public readonly Angle Angle;

    public AdminToyPlaceConstructionGhostRequest(NetCoordinates coordinates, string prototype, Angle angle)
    {
        Coordinates = coordinates;
        Prototype = prototype;
        Angle = angle;
    }
}

[Serializable, NetSerializable]
public sealed class AdminToyClearConstructionGhostRequest : EntityEventArgs
{
    public readonly int GhostId;

    public AdminToyClearConstructionGhostRequest(int ghostId)
    {
        GhostId = ghostId;
    }
}

[Serializable, NetSerializable]
public sealed class AdminToyClearAllConstructionGhostsRequest : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class AdminToyConstructionGhostCreateEvent : EntityEventArgs
{
    public readonly int GhostId;
    public readonly NetCoordinates Coordinates;
    public readonly string Prototype;
    public readonly Angle Angle;

    public AdminToyConstructionGhostCreateEvent(int ghostId, NetCoordinates coordinates, string prototype, Angle angle)
    {
        GhostId = ghostId;
        Coordinates = coordinates;
        Prototype = prototype;
        Angle = angle;
    }
}

[Serializable, NetSerializable]
public sealed class AdminToyClearConstructionGhostsEvent : EntityEventArgs
{
    public readonly int[] GhostIds;

    public AdminToyClearConstructionGhostsEvent(int[] ghostIds)
    {
        GhostIds = ghostIds;
    }
}
