// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeadSpace.Clothing.ReverseRig;

namespace Content.Server.DeadSpace.Clothing.ReverseRig;

/// <summary>
///     Server-side gas bridge for the Reverse RIG backpack. The backpack exposes its own GasTank buffer that
///     is used both for breathing (internals) and the jetpack. Every tick this system tops the buffer up from
///     the oxygen tank inserted in the backpack's item slot, making that tank the actual gas source.
/// </summary>
public sealed class ReverseRigGasBridgeSystem : EntitySystem
{
    public const string TankSlotId = "sor-tank";

    /// <summary>
    ///     Working gas reserve held in the backpack's buffer. The buffer is always topped up to this level
    ///     from the inserted tank, so the tank stays the actual reservoir and drains as gas is consumed.
    /// </summary>
    private const float TargetBufferMoles = 0.5f;

    private const float Epsilon = 0.0001f;

    /// <summary>
    ///     Tolerance for the per-gas composition comparison between the buffer and the inserted tank.
    /// </summary>
    private const float CompositionTolerance = 0.05f;

    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ReverseRigBackpackComponent, GasTankComponent>();
        while (query.MoveNext(out var uid, out _, out var gasTankComp))
        {
            var buffer = gasTankComp.Air;
            if (buffer == null)
                continue;

            // No tank inserted - drain the buffer. Without a tank nothing can breathe or fly.
            if (!_itemSlots.TryGetSlot(uid, TankSlotId, out var slot) || slot.Item is not { } tankUid)
            {
                Drain(buffer);
                continue;
            }

            if (!TryComp<GasTankComponent>(tankUid, out var tankComp) || tankComp.Air == null)
            {
                Drain(buffer);
                continue;
            }

            var tankAir = tankComp.Air;

            // The buffer must always contain the gas from the inserted tank. If the tank was swapped for one
            // holding a different gas, throw away the stale buffer contents before topping up.
            if (!CompositionMatches(buffer, tankAir))
                Drain(buffer);

            // The tank is the gas source: keep the buffer topped up to the working reserve and no more.
            var toAdd = TargetBufferMoles - buffer.TotalMoles;
            if (toAdd <= Epsilon)
                continue;

            var toTransfer = Math.Min(toAdd, tankAir.TotalMoles);
            if (toTransfer <= Epsilon)
                continue;

            _atmos.Merge(buffer, tankAir.Remove(toTransfer));
        }
    }

    private static void Drain(GasMixture buffer)
    {
        if (buffer.TotalMoles > Epsilon)
            buffer.Remove(buffer.TotalMoles);
    }

    private static bool CompositionMatches(GasMixture buffer, GasMixture tank)
    {
        // An empty buffer can not mismatch whatever is inserted.
        if (buffer.TotalMoles <= Epsilon)
            return true;

        // The buffer still holds gas but the inserted tank is empty - stale gas.
        if (tank.TotalMoles <= Epsilon)
            return false;

        var bufferTotal = buffer.TotalMoles;
        var tankTotal = tank.TotalMoles;

        for (var i = 0; i < Atmospherics.AdjustedNumberOfGases; i++)
        {
            var expected = bufferTotal * (tank[i] / tankTotal);
            if (MathF.Abs(buffer[i] - expected) > CompositionTolerance)
                return false;
        }

        return true;
    }
}
