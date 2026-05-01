// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[RegisterComponent]
public sealed partial class ConsoleCraftBlueprintReceiverComponent : Component
{
    public const string ContainerId = "consolecraft_blueprints";

    [DataField]
    public int MaxBlueprints { get; set; } = 0;
}
