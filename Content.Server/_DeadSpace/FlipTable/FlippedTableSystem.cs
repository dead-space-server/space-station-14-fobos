using Content.Shared._DeadSpace.FlipTable;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Verbs;


namespace Content.Server._DeadSpace.FlipTable;

public sealed class FlippedTableSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlippableTableComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerb);
        SubscribeLocalEvent<FlippedTableComponent, GetVerbsEvent<AlternativeVerb>>(OnFlippedGetAltVerb);

        SubscribeLocalEvent<FlippableTableComponent, FlipTableDoAfterEvent>(OnFlipDoAfter);
        SubscribeLocalEvent<FlippedTableComponent, UnflipTableDoAfterEvent>(OnUnflipDoAfter);
    }

    private void OnGetAltVerb(EntityUid uid, FlippableTableComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () => TryFlipTable(uid, component, args.User),
            Text = Loc.GetString("flip-table-verb"),
            Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/fold.svg.192dpi.png")),
            Priority = 2,
        };
        args.Verbs.Add(verb);
    }

    private void OnFlippedGetAltVerb(EntityUid uid, FlippedTableComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () => TryUnflipTable(uid, component, args.User),
            Text = Loc.GetString("unflip-table-verb"),
            Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/fold.svg.192dpi.png")),
            Priority = 2,
        };
        args.Verbs.Add(verb);
    }

    private void TryFlipTable(EntityUid uid, FlippableTableComponent component, EntityUid user)
    {
        var args = new DoAfterArgs(EntityManager, user, component.FlipDelay, new FlipTableDoAfterEvent(), eventTarget: uid, target: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnDamage = true,
            DistanceThreshold = 2.0f,
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void TryUnflipTable(EntityUid uid, FlippedTableComponent component, EntityUid user)
    {
        var delay = 1.0f;
        if (TryComp<FlippableTableComponent>(uid, out var flippable))
            delay = flippable.UnflipDelay;

        var args = new DoAfterArgs(EntityManager, user, delay, new UnflipTableDoAfterEvent(), eventTarget: uid, target: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnDamage = true,
            DistanceThreshold = 2.0f,
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnFlipDoAfter(EntityUid uid, FlippableTableComponent component, FlipTableDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        var xform = Comp<TransformComponent>(uid);
        var flipped = Spawn(component.FlippedTableId, xform.Coordinates);
        var flippedComp = Comp<FlippedTableComponent>(flipped);
        flippedComp.FlipperUid = args.User;

        var flippedXform = Comp<TransformComponent>(flipped);
        _transform.SetLocalRotation(flipped, xform.LocalRotation, flippedXform);

        Del(uid);

        _popup.PopupEntity(Loc.GetString("flip-table-success"), flipped, args.User);
    }

    private void OnUnflipDoAfter(EntityUid uid, FlippedTableComponent component, UnflipTableDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        var xform = Comp<TransformComponent>(uid);
        var original = Spawn(component.OriginalTableId, xform.Coordinates);
        var originalXform = Comp<TransformComponent>(original);
        _transform.SetLocalRotation(original, xform.LocalRotation, originalXform);
        Del(uid);

        _popup.PopupEntity(Loc.GetString("unflip-table-success"), original, args.User);
    }
}
