using Content.Shared.Actions;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Stunnable;
using Content.Server.Chat.Systems;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingScreechSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingScreechComponent, ShadowlingScreechEvent>(OnScreech);
    }

    private void OnScreech(EntityUid uid, ShadowlingScreechComponent component, ShadowlingScreechEvent args)
    {
        if (args.Handled) return;

        _chat.TrySendInGameICMessage(uid, "кричит!", InGameICChatType.Emote, ChatTransmitRange.Normal);

        foreach (var target in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (target == uid) continue;

            if (HasComp<ShadowlingRecruitComponent>(target) ||
                HasComp<ShadowlingSlaveComponent>(target) ||
                HasComp<ShadowlingRevealComponent>(target) ||
                HasComp<ShadowlingComponent>(target))
                continue;

            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(component.StunDuration));
        }

        args.Handled = true;
    }
}
