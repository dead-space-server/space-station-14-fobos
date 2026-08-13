using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.Antags.Abductor.Components;

[Serializable, NetSerializable]
public enum AbductorOrganType : byte
{
    None,
    Health,
    NitrousOxide,
    Gravity,
    Egg,
    Spider
}
