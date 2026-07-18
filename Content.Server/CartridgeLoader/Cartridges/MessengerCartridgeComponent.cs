namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class MessengerCartridgeComponent : Component
{
    public int? ActiveChatPartnerId;
    public Dictionary<int, TimeSpan> LastMessageTime = new(); //DS14
    public HashSet<int> BlockedUsers = new(); //DS14
    public bool IncomingMessagesDisabled; //DS14
}
