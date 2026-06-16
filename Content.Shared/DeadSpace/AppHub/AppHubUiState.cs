using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AppHub;

[Serializable, NetSerializable]
public sealed class AppHubUiState : BoundUserInterfaceState
{
    public bool HasPda;
    public int UsedDiskSpace;
    public int MaxDiskSpace;
    public string SelectedCategory;
    public List<AppHubCatalogEntry> CatalogEntries;

    public AppHubUiState(
        bool hasPda,
        int usedDiskSpace,
        int maxDiskSpace,
        string selectedCategory,
        List<AppHubCatalogEntry> catalogEntries)
    {
        HasPda = hasPda;
        UsedDiskSpace = usedDiskSpace;
        MaxDiskSpace = maxDiskSpace;
        SelectedCategory = selectedCategory;
        CatalogEntries = catalogEntries;
    }
}

[Serializable, NetSerializable]
public sealed class AppHubCatalogEntry
{
    public string Id = string.Empty;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public string Category = string.Empty;
    public bool IsInstalled;
}
