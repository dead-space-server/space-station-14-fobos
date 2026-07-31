using Robust.Shared.Audio;

namespace Content.Shared.Mobs;

/// <summary>
///     DS14: Метка для одежды (противогазов), которая должна
///     заменять стандартный звук смерти (deathgasp) кастомный
/// </summary>
[RegisterComponent]
public sealed partial class SpecialDeathSoundComponent : Component
{
    [DataField("sound", required: true)]
    public SoundSpecifier Sound = default!;
}
