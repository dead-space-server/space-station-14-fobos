using Robust.Shared.Prototypes;

namespace Content.Shared._Frostheim.Map;

[RegisterComponent]
public sealed partial class FrostheimLootSpawnComponent : Component
{
    [DataField]
    public FrostheimLootType LootType = FrostheimLootType.Any;
}

public enum FrostheimLootType : byte
{
    Any,
    Thruster,
    Gyroscope
}
