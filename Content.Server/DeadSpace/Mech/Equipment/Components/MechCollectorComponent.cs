using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.Mech.Equipment.Components;

[RegisterComponent]
public sealed partial class MechCollectorComponent : Component
{
    [DataField("scanInterval")]
    public TimeSpan ScanInterval = TimeSpan.FromSeconds(1);

    [DataField("nextScan")]
    public TimeSpan NextScan = TimeSpan.Zero;

    [DataField("range")]
    public float Range = 1.5f;

    [DataField("collectEnergyDelta")]
    public float CollectEnergyDelta = -10f;

    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_DeadSpace/Mecha/sound_mecha_powerloader_turn2.ogg");
}
