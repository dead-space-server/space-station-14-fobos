using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Stunnable;
using Content.Server.Chat.Systems;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingBlinkSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!; 
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingBlinkComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingBlinkComponent, ShadowlingBlinkEvent>(OnBlinkAction);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingBlinkComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionBlinkEntity, component.ActionBlink);
    }

    private void OnBlinkAction(EntityUid uid, ShadowlingBlinkComponent component, ShadowlingBlinkEvent args)
    {
        if (args.Handled) return;

        _chat.TrySendInGameICMessage(uid, "кричит!", InGameICChatType.Emote, ChatTransmitRange.Normal);

        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(5));

        args.Handled = true;
    }
}
