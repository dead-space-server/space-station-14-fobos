using Robust.Shared.Serialization;

namespace Content.Shared._Frostheim.Creatures;

[RegisterComponent]
public sealed partial class FrostheimCreatureComponent : Component
{
    [DataField]
    public FrostheimCreatureType CreatureType = FrostheimCreatureType.Slasher;
}

[Serializable, NetSerializable]
public enum FrostheimCreatureType : byte
{
    Slasher,
    Lurker,
    Swarm,
    Brute
}
