using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.Lavaland;

[Serializable, NetSerializable]
public enum LavalandAshDrakeVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum LavalandAshDrakeVisualLayers : byte
{
    Base,
}

[Serializable, NetSerializable]
public enum LavalandAshDrakeVisualState : byte
{
    Dragon,
    Shadow,
    Swoop,
    Dead,
}
