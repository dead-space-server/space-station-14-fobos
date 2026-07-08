// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Client.Instruments;
using Content.Shared.DeadSpace.Instruments;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Content.Shared.Inventory.Events;

namespace Content.Client.DeadSpace.Instruments;

public sealed class NoiseCancellingClientSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly InstrumentSystem _instrumentSystem = default!;


    private const float MutedMultiplier = 0.5f;
    private const byte MidiVolumeBoost = 110;

    private bool _isActive;
    private static readonly string[] HeadphonesSlots = ["ears", "head"];
    private readonly Dictionary<EntityUid, float> _applied = [];

    public override void Initialize()
    {
        base.Initialize();
        UpdatesAfter.Add(typeof(AudioSystem));
        SubscribeLocalEvent<AudioComponent, ComponentInit>(OnAudioInit, before: [typeof(AudioSystem)]);
        SubscribeLocalEvent<HeadphonesInstrumentComponent, GotUnequippedEvent>(OnHeadphonesUnequipped);
    }

    private void OnHeadphonesUnequipped(EntityUid uid, HeadphonesInstrumentComponent comp, ref GotUnequippedEvent args)
    {
        var local = _playerManager.LocalEntity;
        if (local == null || args.Equipee != local.Value)
            return;

        if (TryComp<InstrumentComponent>(uid, out var instr))
            _instrumentSystem.EndRenderer(uid, false, instr);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var localPlayer = _playerManager.LocalPlayer?.ControlledEntity;
        var shouldBeActive = false;
        EntityUid? activeHeadphones = null;

        if (localPlayer != null)
        {
            foreach (var slot in HeadphonesSlots)
            {
                if (_inventory.TryGetSlotEntity(localPlayer.Value, slot, out var Headphones) &&
                    HasComp<HeadphonesInstrumentComponent>(Headphones.Value) &&
                    TryComp<ItemToggleComponent>(Headphones.Value, out var toggle) &&
                    toggle.Activated)
                {
                    shouldBeActive = true;
                    activeHeadphones = Headphones.Value;
                    break;
                }
            }
        }

        if (shouldBeActive != _isActive)
        {
            _isActive = shouldBeActive;

            if (_isActive)
            {
                if (activeHeadphones != null)
                    ApplyMidiBoost(activeHeadphones.Value);
            }
            else
            {
                _applied.Clear();

                if (localPlayer != null)
                {
                    foreach (var slot in HeadphonesSlots)
                    {
                        if (_inventory.TryGetSlotEntity(localPlayer.Value, slot, out var Headphones) &&
                            HasComp<HeadphonesInstrumentComponent>(Headphones.Value))
                        {
                            RemoveMidiBoost(Headphones.Value);
                            break;
                        }
                    }
                }
            }
        }

        if (!_isActive)
            return;

        if (activeHeadphones != null)
            ApplyMidiBoost(activeHeadphones.Value);

        var query = EntityQueryEnumerator<AudioComponent>();
        while (query.MoveNext(out var uid, out var audioComp))
        {
            TryApplyReduction(uid, audioComp);
        }

        if (_applied.Count > 0)
        {
            var toRemove = new List<EntityUid>();
            foreach (var uid in _applied.Keys)
            {
                if (TerminatingOrDeleted(uid))
                    toRemove.Add(uid);
            }
            foreach (var uid in toRemove)
                _applied.Remove(uid);
        }
    }

    private void OnAudioInit(EntityUid uid, AudioComponent audioComp, ComponentInit args)
    {
        if (!_isActive)
            return;

        audioComp.Gain *= MutedMultiplier;
        _applied[uid] = audioComp.Gain;
    }

    private void TryApplyReduction(EntityUid uid, AudioComponent audioComp)
    {
        var currentGain = audioComp.Gain;

        if (_applied.TryGetValue(uid, out var lastApplied) &&
            MathF.Abs(currentGain - lastApplied) < 0.001f)
        {
            return;
        }

        var reduced = currentGain * MutedMultiplier;
        audioComp.Gain = reduced;
        _applied[uid] = reduced;
    }

    private void ApplyMidiBoost(EntityUid Headphones)
    {
        if (!TryComp<InstrumentComponent>(Headphones, out var instr) || instr.Renderer == null)
            return;

        instr.Renderer.MinVolume = MidiVolumeBoost;
    }

    private void RemoveMidiBoost(EntityUid Headphones)
    {
        if (!TryComp<InstrumentComponent>(Headphones, out var instr) || instr.Renderer == null)
            return;

        instr.Renderer.MinVolume = 0;
    }
}
