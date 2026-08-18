using Content.Shared.DeadSpace.Events;
using Content.Shared.PDA;
using Content.Shared.PDA.Ringer;
using Content.Shared.Store.Components;
using Robust.Client.Audio.Midi;
using Robust.Shared.Audio.Midi;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.PDA.Ringer;

/// <summary>
/// Handles the client-side logic for <see cref="SharedRingerSystem"/>.
/// </summary>
public sealed class RingerSystem : SharedRingerSystem
{
    // DS14-Start
    [Dependency] private readonly IMidiManager _midiManager = default!;

    private readonly Dictionary<NetEntity, IMidiRenderer> _activeMidiRingtoneRenderers = new();
    // DS14-End

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RingerComponent, AfterAutoHandleStateEvent>(OnRingerUpdate);

        // DS14-Start
        SubscribeNetworkEvent<RingerPlayMidiRingtoneEvent>(OnMidiRingtoneEvent);
        // DS14-End
    }

    /// <summary>
    /// Updates the UI whenever we get a new component state from the server.
    /// </summary>
    private void OnRingerUpdate(Entity<RingerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateRingerUi(ent);
    }

    /// <inheritdoc/>
    protected override void UpdateRingerUi(Entity<RingerComponent> ent)
    {
        if (UI.TryGetOpenUi(ent.Owner, RingerUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }

    // DS14-Start
    /// <summary>
    /// миди рендер и остановка миди через 3 секунды
    /// </summary>
    private void OnMidiRingtoneEvent(RingerPlayMidiRingtoneEvent ev)
    {
        var uid = GetEntity(ev.Uid);

        if (uid == EntityUid.Invalid || !HasComp<RingerComponent>(uid))
            return;

        if (_activeMidiRingtoneRenderers.TryGetValue(ev.Uid, out var existing))
        {
            existing.StopAllNotes();
            existing.CloseMidi();
            existing.Dispose();
            _activeMidiRingtoneRenderers.Remove(ev.Uid);
        }

        if (ev.MidiData.Length == 0)
            return;

        var renderer = _midiManager.GetNewRenderer(mono: false);
        if (renderer == null)
            return;

        renderer.TrackingEntity = uid;
        renderer.Mono = false;
        renderer.DisableProgramChangeEvent = false;
        renderer.SendMidiEvent(RobustMidiEvent.SystemReset(renderer.SequencerTick));

        if (!renderer.OpenMidi(ev.MidiData))
        {
            renderer.Dispose();
            return;
        }

        _activeMidiRingtoneRenderers[ev.Uid] = renderer;

        // килл миди через 3 секунды
        Timer.Spawn(3500, () =>
        {
            if (renderer.Disposed)
                return;

            renderer.StopAllNotes();
            renderer.CloseMidi();
            renderer.Dispose();
            _activeMidiRingtoneRenderers.Remove(ev.Uid);
        });
    }

    public override void Shutdown()
    {
        base.Shutdown();

        foreach (var renderer in _activeMidiRingtoneRenderers.Values)
        {
            if (!renderer.Disposed)
            {
                renderer.StopAllNotes();
                renderer.CloseMidi();
                renderer.Dispose();
            }
        }
        _activeMidiRingtoneRenderers.Clear();
    }
    // DS14-End
}
