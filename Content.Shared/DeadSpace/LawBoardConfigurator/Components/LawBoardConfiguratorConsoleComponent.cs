using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.LawBoardConfigurator.Components;

[RegisterComponent]
public sealed partial class LawBoardConfiguratorConsoleComponent : Component
{
    [DataField]
    public string BoardSlot = "circuit_holder";

    [DataField]
    public SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/Machines/Keyboard/keyboard1.ogg");
}
