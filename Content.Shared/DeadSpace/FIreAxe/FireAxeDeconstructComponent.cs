// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.FireAxe;

[RegisterComponent, NetworkedComponent]
public sealed partial class FireAxeDeconstructComponent : Component
{
    [DataField]
    public float DeconstructDelay = 5f;
}

[Serializable, NetSerializable]
public sealed partial class FireAxeDeconstructDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public NetCoordinates Location;

    [DataField]
    public NetEntity GridId;

    public FireAxeDeconstructDoAfterEvent() { }

    public FireAxeDeconstructDoAfterEvent(NetCoordinates location, NetEntity gridId)
    {
        Location = location;
        GridId = gridId;
    }
}