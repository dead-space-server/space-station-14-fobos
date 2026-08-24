using System.Linq;
using Content.Server.Store.Systems;
using Content.Shared.Changeling.Components;
using Content.Shared.Store.Components;

namespace Content.Server.Changeling.Systems;

/// <summary>
/// Awards DNA currency after a successful unique devour.
/// </summary>
public sealed class ChangelingDnaSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        // DS14: StoreSystem is server-only on this content baseline.
        SubscribeLocalEvent<ChangelingDevouredEvent>(OnDevoured);
    }

    private void OnDevoured(ref ChangelingDevouredEvent args)
    {
        if (!args.GrantedDna ||
            !TryComp<ChangelingDevourComponent>(args.Changeling, out var devour) ||
            !TryComp<StoreComponent>(args.Changeling, out var store))
        {
            return;
        }

        var reward = devour.DevourDnaReward.ToDictionary(pair => pair.Key.Id, pair => pair.Value);
        _store.TryAddCurrency(reward, args.Changeling, store);
    }
}
