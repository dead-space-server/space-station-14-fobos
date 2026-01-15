using Content.Server.Chat.Systems;
using Content.Server.DeadSpace.MartialArts.SmokingCarp.Components;
using Content.Shared.DeadSpace.MartialArts.SmokingCarp;

namespace Content.Server.DeadSpace.MartialArts;

public sealed class MartialArtsChatMessageSystem : SharedMartialArtsSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokingCarpComponent, SmokingCarpSaying>(OnSmokingCarpSaying);
    }

    private void OnSmokingCarpSaying(Entity<SmokingCarpComponent> ent, ref SmokingCarpSaying args)
    {
        _chat.TrySendInGameICMessage(ent, Loc.GetString(args.Saying), InGameICChatType.Speak, false);
    }
}
