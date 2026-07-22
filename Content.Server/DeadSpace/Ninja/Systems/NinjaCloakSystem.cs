using Content.Shared.DeadSpace.Ninja.Systems;
using Content.Shared.DeadSpace.Ninja.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;

namespace Content.Server.DeadSpace.Ninja.Systems;

public sealed class NinjaCloakSystem : SharedNinjaCloakSystem
{
    [Dependency] private readonly NinjaSmokeAbilitySystem _ninjaSmoke = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    protected override void AfterToggleCloak(Entity<SpaceNinjaComponent> ent, EntityUid suitUid, NinjaCloakComponent cloak)
    {
        base.AfterToggleCloak(ent, suitUid, cloak);

        if (cloak.Enabled)
        {
            if (TryComp<NinjaSmokeAbilityComponent>(suitUid, out var smokeComp))
            {
                _ninjaSmoke.TrySpawnNinjaSmoke((suitUid, smokeComp), true);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = AllEntityQuery<NinjaCloakComponent>();
        while (query.MoveNext(out var uid, out var cloak))
        {
            if (!cloak.Enabled)
                continue;

            if (!_itemSlots.TryGetSlot(uid, "cell_slot", out var slot) || slot.Item is not { } batteryUid)
            {
                cloak.Enabled = false;
                Dirty(uid, cloak);
                continue;
            }

            float cost = cloak.DrainRate * frameTime;

            if (!_battery.TryUseCharge(batteryUid, cost))
            {
                cloak.Enabled = false;
                Dirty(uid, cloak);
            }
        }
    }

}