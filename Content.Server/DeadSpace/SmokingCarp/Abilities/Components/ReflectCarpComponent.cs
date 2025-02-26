using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.DeadSpace.SmokingCarp.Abilities.Components;

[RegisterComponent]
public sealed partial class ReflectCarpComponent : Component
{
    [DataField]
    public EntityUid? ActionReflectCarpEntity;

    [DataField("actionReflectCarp", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionReflectCarp = "ActionReflectCarp";
}
