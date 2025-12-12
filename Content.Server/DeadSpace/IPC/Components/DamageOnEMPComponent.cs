// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.IPC.Components;

[RegisterComponent]
public sealed partial class DamageOnEMPComponent : Component
{
    [DataField]
    public float Damage = 10f;

    [DataField]
    public ProtoId<DamageTypePrototype> DamageType = "Shock";
}
