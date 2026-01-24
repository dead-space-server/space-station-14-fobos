// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT
using Robust.Shared.GameStates;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;

namespace Content.Server.DeadSpace.AntiAlcohol;

[RegisterComponent]
public sealed partial class AntiAlcoholWatcherComponent : Component
{
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Poison", 1 }
        }
    };
}
