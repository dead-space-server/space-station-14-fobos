using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AppHub;

[Serializable, NetSerializable]
public enum AppHubUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class AppHubInstallMessage : BoundUserInterfaceMessage
{
    public string ProgramId;

    public AppHubInstallMessage(string programId)
    {
        ProgramId = programId;
    }
}

[Serializable, NetSerializable]
public sealed class AppHubSelectCategoryMessage : BoundUserInterfaceMessage
{
    public string Category;

    public AppHubSelectCategoryMessage(string category)
    {
        Category = category;
    }
}

[Serializable, NetSerializable]
public sealed class AppHubUninstallMessage : BoundUserInterfaceMessage
{
    public string ProgramId;

    public AppHubUninstallMessage(string programId)
    {
        ProgramId = programId;
    }
}

[Serializable, NetSerializable]
public sealed class AppHubEjectPdaMessage : BoundUserInterfaceMessage { }
