// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Content.Server.DeadSpace.MartialArts.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Arkalyse.Components;

[RegisterComponent]
public sealed partial class ArkalyseGlovesComponent : Component
{
    [DataField]
    public ArkalyseParams Params = new();

    // Хранит EntityUid выданных экшенов для точного отзыва при снятии
    [DataField]
    public List<EntityUid> GrantedActions = new();

    public bool AddedArkalyseComponent;
}