using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.DeviceLinking;

namespace Content.Server.Research.Components;

[RegisterComponent]
public sealed partial class TechDiskPrinterOnSignalComponent : Component
{
    /// <summary>
    /// Порт, при получении сигнала на который начинается печать диска
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> PrintPort;
}
