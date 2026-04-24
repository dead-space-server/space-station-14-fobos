// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Audio;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[RegisterComponent]
public sealed partial class ConsoleCraftStationComponent : Component
{
    public const string ContainerName = "consolecraft_items";

    public TimeSpan PackEndTime;

    [ViewVariables]
    public EntityUid? CraftingSoundEntity;

    [ViewVariables]
    public Container ItemContainer = default!;

    [ViewVariables]
    public Dictionary<string, List<EntityUid>> InsertedRequired { get; set; } = new();

    [ViewVariables]
    public Dictionary<string, EntityUid> InsertedModules { get; set; } = new();

    [ViewVariables]
    public Dictionary<int, List<EntityUid>> InsertedRandomRequired { get; set; } = new();

    [ViewVariables]
    public Dictionary<int, string> ChosenRandomItems { get; set; } = new();

    [DataField]
    public string? ActiveRecipeId { get; set; }

    [DataField]
    public bool CraftInProgress { get; set; } = false;

    [DataField]
    public EntityUid? LinkedConsole { get; set; }

    [DataField]
    public SoundSpecifier? CraftingSound = new SoundPathSpecifier("/Audio/_DeadSpace/Machines/stanok.ogg");
}

[RegisterComponent]
public sealed partial class ConsoleCraftConsoleComponent : Component
{
    [DataField]
    public string? SelectedBlueprintId { get; set; }

    [DataField]
    public EntityUid? LinkedStation { get; set; }

    public bool ShowingList = true;

    public Dictionary<string, Dictionary<int, string>> SavedRandomChoices { get; set; } = new();
}

[RegisterComponent]
public sealed partial class ConsoleCraftLinkingComponent : Component
{
    public EntityUid ConsoleUid { get; set; }
}