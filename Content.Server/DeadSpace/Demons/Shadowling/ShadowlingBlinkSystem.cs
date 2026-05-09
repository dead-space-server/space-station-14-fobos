// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Stunnable;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingBlinkSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

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

        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(component.StunDuration));

        args.Handled = true;
    }
}