using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.Mech.Equipment.Components;

[RegisterComponent]
public sealed partial class MechCollectorComponent : Component
{
    [DataField]
    public TimeSpan ScanInterval = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextScan = TimeSpan.Zero;

    [DataField]
    public float Range = 1.5f;

    [DataField]
    public float CollectEnergyDelta = 10f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_DeadSpace/Mecha/sound_mecha_powerloader_turn2.ogg");
}
