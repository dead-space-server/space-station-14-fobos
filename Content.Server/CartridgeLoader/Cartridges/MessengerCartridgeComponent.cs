namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class MessengerCartridgeComponent : Component
{
    public int? ActiveChatPartnerId;
    public Dictionary<int, TimeSpan> LastMessageTime = new(); //DS14
}