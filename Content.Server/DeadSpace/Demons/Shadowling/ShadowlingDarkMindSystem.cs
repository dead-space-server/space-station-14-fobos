using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Mobs;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingDarkMindSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingDarkMindComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingDarkMindComponent, ShadowlingDarkMindEvent>(OnDarkMind);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingDarkMindComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionDarkMindEntity, component.ActionDarkMind);
    }

    private void OnDarkMind(EntityUid uid, ShadowlingDarkMindComponent component, ShadowlingDarkMindEvent args)
    {
        if (args.Handled) return;

        int personalSlaves = 0;
        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();

        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master == uid && _mobState.IsAlive(sUid))
            {
                personalSlaves++;
            }
        }

        _popup.PopupEntity($"У вас {personalSlaves} живых порабощённых.", uid, uid, PopupType.Medium);

        args.Handled = true;
    }
}
