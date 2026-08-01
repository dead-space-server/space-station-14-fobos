using Content.Server.Chat.Systems;
using Content.Server.Speech.Muting;
using Content.Shared.Inventory; // DS14
using Content.Shared.Mobs;
using Content.Shared.Speech.Muting;
using Robust.Shared.Audio.Systems; // DS14
using Robust.Shared.Prototypes;

namespace Content.Server.Mobs;

/// <see cref="DeathgaspComponent"/>
public sealed class DeathgaspSystem: EntitySystem
public sealed class DeathgaspSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // DS14
    [Dependency] private readonly SharedAudioSystem _audio = default!; // DS14

    public override void Initialize()
    {
    private void OnMobStateChanged(EntityUid uid, DeathgaspComponent component, MobStateChangedEvent args)
    {
        // don't deathgasp if they arent going straight from crit to dead
        if (args.NewMobState != MobState.Dead  args.OldMobState is not (MobState.Critical or MobState.PreCritical)) // DS14 edited
        if (args.NewMobState != MobState.Dead
            args.OldMobState is not (MobState.Critical or MobState.PreCritical))
            return;

    // DS14-start
        if (_inventory.TryGetSlotEntity(uid, "mask", out var maskUid) &&
            TryComp<SpecialDeathSoundComponent>(maskUid, out var special))
        {
            _audio.PlayPvs(special.Sound, uid);

            Deathgasp(uid, component);
            return;
        }
     // DS14-end
        Deathgasp(uid, component);
    }
    /// <summary>
    ///     Causes an entity to perform their deathgasp emote, if they have one.
    /// </summary>
    ///
