using Content.Shared.Item.ItemToggle;
using Content.Shared.PowerCell;
using Content.Shared.Tag; // DS14
using Content.Shared.Weapons.Misc;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes; // DS14

namespace Content.Server.Weapons.Misc;

public sealed partial class TetherGunSystem : SharedTetherGunSystem
{
    [Dependency] private PowerCellSystem _cell = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;

    private static readonly ProtoId<TagPrototype> CannotTetherTag = "CannotTether"; // DS14

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TetherGunComponent, PowerCellSlotEmptyEvent>(OnGunEmpty);
        SubscribeLocalEvent<ForceGunComponent, PowerCellSlotEmptyEvent>(OnGunEmpty);
    }

    private void OnGunEmpty(EntityUid uid, BaseForceGunComponent component, ref PowerCellSlotEmptyEvent args)
    {
        StopTether(uid, component);
    }

    protected override bool CanTether(EntityUid uid, BaseForceGunComponent component, EntityUid target, EntityUid? user)
    {
        if (!base.CanTether(uid, component, target, user))
            return false;

        if (_tagSystem.HasTag(target, CannotTetherTag)) // DS14
            return false; // DS14

        if (!_cell.HasDrawCharge(uid, user: user))
            return false;

        return true;
    }

    protected override void StartTether(EntityUid gunUid, BaseForceGunComponent component, EntityUid target, EntityUid? user,
        PhysicsComponent? targetPhysics = null, TransformComponent? targetXform = null)
    {
        base.StartTether(gunUid, component, target, user, targetPhysics, targetXform);
        _toggle.TryActivate(gunUid);
    }

    protected override void StopTether(EntityUid gunUid, BaseForceGunComponent component, bool land = true, bool transfer = false)
    {
        base.StopTether(gunUid, component, land, transfer);
        _toggle.TryDeactivate(gunUid);
    }
}
