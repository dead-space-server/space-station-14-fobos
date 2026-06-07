using Content.Shared.Alert;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing; // DS14
using Content.Shared.Gravity; // DS14
using Content.Shared.DeadSpace.Movement.Components; // DS14
using Content.Shared.Stunnable;
using Robust.Shared.Timing; // DS14

namespace Content.Shared.Movement.Systems;

/// <summary>
/// This handles the worm component
/// </summary>
public sealed class WormSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!; //DS14
    [Dependency] private readonly IGameTiming _timing = default!; //DS14

    public override void Initialize()
    {
        SubscribeLocalEvent<WormComponent, StandUpAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<WormComponent, KnockedDownRefreshEvent>(OnKnockedDownRefresh);
        SubscribeLocalEvent<WormComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<WormComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WormComponent, WeightlessnessChangedEvent>(OnWeightlessnessChanged); // DS14
        SubscribeLocalEvent<WormComponent, ComponentShutdown>(OnShutdown); // DS14
    }

    private void OnMapInit(Entity<WormComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<KnockedDownComponent>(ent, out var knocked);
        _alerts.ShowAlert(ent.Owner, SharedStunSystem.KnockdownAlert);
        _stun.SetAutoStand((ent, knocked));
    }

    private void OnRejuvenate(Entity<WormComponent> ent, ref RejuvenateEvent args)
    {
        RemComp<WormComponent>(ent);
    }

    private void OnStandAttempt(Entity<WormComponent> ent, ref StandUpAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = true;
        args.Message = (Loc.GetString("worm-component-stand-attempt"), PopupType.SmallCaution);
        args.Autostand = false;
    }

    private void OnKnockedDownRefresh(Entity<WormComponent> ent, ref KnockedDownRefreshEvent args)
    {
        args.FrictionModifier *= ent.Comp.FrictionModifier;
        args.SpeedModifier *= ent.Comp.SpeedModifier;
    }
//DS14-start
    private void OnWeightlessnessChanged(Entity<WormComponent> ent, ref WeightlessnessChangedEvent args)
    {
        if (args.Weightless)
            return;

        if (!HasComp<WheelchairUserComponent>(ent.Owner))
            return;

        if (_timing.ApplyingState)
            return;

        EnsureComp<KnockedDownComponent>(ent, out var knocked);
        _stun.SetAutoStand((ent, knocked), false);
        _standing.Down(ent.Owner);
    }

    private void OnShutdown(Entity<WormComponent> ent, ref ComponentShutdown args)
    {
        RemComp<KnockedDownComponent>(ent.Owner);
        _alerts.ClearAlert(ent.Owner, SharedStunSystem.KnockdownAlert);
    }
//DS14-end
}