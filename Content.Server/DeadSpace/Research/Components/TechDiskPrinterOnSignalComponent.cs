using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.Research.Components;

[RegisterComponent]
public sealed partial class TechDiskPrinterOnSignalComponent : Component
{
    [DataField("printPort")]
    public string PrintPort = "On";
}
