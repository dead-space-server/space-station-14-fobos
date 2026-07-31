using Robust.Shared.Audio;

namespace Content.Server.Mobs;

/// <summary>
///     DS14: Метка для одежды (например, противогазов), которая должна
///     заменять стандартный звук смерти (deathgasp) кастомным.
/// </summary>
[RegisterComponent]
public sealed partial class SpecialDeathSoundComponent : Component
{
    [DataField("sound", required: true)]
    public SoundSpecifier Sound = default!;
}
