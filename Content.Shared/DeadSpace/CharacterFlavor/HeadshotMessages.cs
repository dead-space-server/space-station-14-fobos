using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.CharacterFlavor;

[Serializable, NetSerializable]
public sealed partial class RequestHeadshotDownloadEvent : EntityEventArgs
{
    public readonly string Url;
    public RequestHeadshotDownloadEvent(string url)
    {
        Url = url;
    }
}

[Serializable, NetSerializable]
public sealed partial class HeadshotDownloadResultEvent : EntityEventArgs
{
    public readonly string? Base64;
    public readonly bool Success;
    public HeadshotDownloadResultEvent(string? base64, bool success)
    {
        Base64 = base64;
        Success = success;
    }
}

[Serializable, NetSerializable]
public sealed partial class RequestHeadshotExamineEvent : EntityEventArgs
{
    public readonly NetEntity Target;
    public RequestHeadshotExamineEvent(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed partial class HeadshotExamineResultEvent : EntityEventArgs
{
    public readonly NetEntity Target;
    public readonly byte[]? Image;
    public readonly string FlavorText;
    public HeadshotExamineResultEvent(NetEntity target, byte[]? image, string flavorText)
    {
        Target = target;
        Image = image;
        FlavorText = flavorText;
    }
}
