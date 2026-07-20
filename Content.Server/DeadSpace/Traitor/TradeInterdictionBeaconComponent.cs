// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Audio;

namespace Content.Server.DeadSpace.Traitor;

[RegisterComponent, Access(typeof(TradeInterdictionBeaconSystem))]
public sealed partial class TradeInterdictionBeaconComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromMinutes(3);

    public TimeSpan? CompletionTime;

    [DataField]
    public SoundSpecifier AlertSound = new SoundPathSpecifier("/Audio/Effects/alert.ogg");

    public EntityUid? TargetStation;
    public EntityUid? TargetTradeGrid;
    public EntityUid? HijackerMind;
    public bool Active;
    public bool Completed;
}
