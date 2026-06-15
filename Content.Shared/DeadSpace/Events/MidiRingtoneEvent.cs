using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Events;

[Serializable, NetSerializable]
public sealed class RingerPlayMidiRingtoneEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public byte[] MidiData { get; }

    public RingerPlayMidiRingtoneEvent(NetEntity uid, byte[] midiData)
    {
        Uid = uid;
        MidiData = midiData;
    }
}
