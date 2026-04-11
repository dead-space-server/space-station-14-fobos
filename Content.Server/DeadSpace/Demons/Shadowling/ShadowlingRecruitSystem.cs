using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mindshield.Components;
using Content.Server.Mind;
using Content.Shared.DeadSpace.Demons.Shadowling;
using Content.Shared.Stunnable;
using Content.Server.Roles;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Robust.Shared.Audio;
using Content.Server.Antag;

namespace Content.Server.DeadSpace.Demons.Shadowling;

public sealed class ShadowlingRecruitSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingRecruitComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ShadowlingRecruitComponent, ShadowlingRecruitEvent>(OnRecruitAction);
        SubscribeLocalEvent<ShadowlingRecruitComponent, ShadowlingRecruitDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<ShadowlingRecruitComponent, MobStateChangedEvent>(OnMasterStateChanged);
        SubscribeLocalEvent<ShadowlingSlaveComponent, MobStateChangedEvent>(OnSlaveStateChanged);
        SubscribeLocalEvent<MindShieldComponent, ComponentInit>(OnMindShieldImplanted);
        SubscribeLocalEvent<ShadowlingSlaveComponent, ComponentShutdown>(OnSlaveRemoved);
        SubscribeLocalEvent<ShadowlingSlaveComponent, ComponentStartup>(OnSlaveStartup);

        SubscribeLocalEvent<ShadowlingScreechComponent, ComponentStartup>(OnAbilityStartup);
        SubscribeLocalEvent<ShadowlingFreezingVeinsComponent, ComponentStartup>(OnAbilityStartup);
        SubscribeLocalEvent<ShadowlingBlackMedComponent, ComponentStartup>(OnAbilityStartup);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, ComponentStartup>(OnAbilityStartup);
    }

    private void OnAbilityStartup(EntityUid uid, IComponent component, ComponentStartup args)
    {
        if (TryComp<ShadowlingRecruitComponent>(uid, out var recruit))
            UpdateSlaveCount(uid, recruit);
    }

    private void OnComponentInit(EntityUid uid, ShadowlingRecruitComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionRecruitEntity, component.ActionRecruit);
        UpdateSlaveCount(uid, component);
    }

    private void OnSlaveStartup(EntityUid uid, ShadowlingSlaveComponent component, ComponentStartup args)
    {
        UpdateAllRecruiters();
    }

    private void OnMasterStateChanged(EntityUid uid, ShadowlingRecruitComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead) return;
        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();
        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master != uid) continue;
            _stun.TryUpdateParalyzeDuration(sUid, TimeSpan.FromSeconds(10));
            if (_mind.TryGetMind(sUid, out var mindId, out _))
                _role.MindRemoveRole(mindId, "MindRoleShadowlingSlave");
            RemCompDeferred<ShadowlingSlaveComponent>(sUid);
        }
    }

    private void OnSlaveStateChanged(EntityUid uid, ShadowlingSlaveComponent component, MobStateChangedEvent args)
    {
        UpdateAllRecruiters();
    }

    private void OnRecruitAction(EntityUid uid, ShadowlingRecruitComponent component, ShadowlingRecruitEvent args)
    {
        if (args.Handled) return;
        var target = args.Target;

        if (HasComp<ShadowlingRecruitComponent>(target) || HasComp<ShadowlingRevealComponent>(target))
        {
            _popup.PopupEntity("Вы не можете поработить другого тенеморфа!", uid, uid, PopupType.Medium);
            return;
        }
        if (TryComp<MetaDataComponent>(target, out var meta) && meta.EntityPrototype?.ID == "MobHumanDeathSquadUnit")
        {
            _popup.PopupEntity("Его воля сопротивляется!", uid, uid, PopupType.Medium);
            return;
        }
        if (HasComp<ShadowlingSlaveComponent>(target))
        {
            _popup.PopupEntity("Разум этой цели уже принадлежит тьме!", uid, uid, PopupType.Medium);
            return;
        }
        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity("Разум цели защищён имплантом!", uid, uid, PopupType.Medium);
            return;
        }
        if (_mobState.IsDead(target) || _mobState.IsCritical(target))
        {
            _popup.PopupEntity("Цель должна быть в сознании!", uid, uid, PopupType.Medium);
            return;
        }
        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity("Этот разум слишком примитивен для порабощения.", uid, uid, PopupType.Medium);
            return;
        }

        args.Handled = true;
        _popup.PopupEntity("Вы шепчете ужасающие истины в разум жертвы...", uid, uid, PopupType.Medium);
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.Duration, new ShadowlingRecruitDoAfterEvent(), uid, target: target)
        {
            BreakOnMove = true,
            NeedHand = true,
            DistanceThreshold = 2f
        };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(EntityUid uid, ShadowlingRecruitComponent component, ShadowlingRecruitDoAfterEvent args)
    {
        var target = args.Args.Target ?? args.Target;
        if (args.Cancelled || target == null) return;

        var targetUid = target.Value;
        var slave = EnsureComp<ShadowlingSlaveComponent>(targetUid);
        slave.Master = uid;

        if (_mind.TryGetMind(targetUid, out var mindId, out var mind))
        {
            _role.MindAddRole(mindId, "MindRoleShadowlingSlave", mind);
            var sound = new SoundPathSpecifier("/Audio/Misc/narsie_rises.ogg");
            _antag.SendBriefing(targetUid, Loc.GetString("roles-antag-shadowlingslave-objective"), Color.Red, sound);
        }

        UpdateSlaveCount(uid, component);
    }

    private void OnMindShieldImplanted(EntityUid uid, MindShieldComponent comp, ComponentInit args)
    {
        if (HasComp<ShadowlingSlaveComponent>(uid))
        {
            _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(10));
            if (_mind.TryGetMind(uid, out var mindId, out _))
                _role.MindRemoveRole(mindId, "MindRoleShadowlingSlave");
            RemComp<ShadowlingSlaveComponent>(uid);
            UpdateAllRecruiters();
        }
    }

    private void OnSlaveRemoved(EntityUid uid, ShadowlingSlaveComponent component, ComponentShutdown args)
    {
        UpdateAllRecruiters();
    }

    public void UpdateAllRecruiters()
    {
        var query = EntityQueryEnumerator<ShadowlingRecruitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateSlaveCount(uid, comp);
        }
    }

    public void UpdateSlaveCount(EntityUid uid, ShadowlingRecruitComponent component)
    {
        var count = 0;
        var query = EntityQueryEnumerator<ShadowlingSlaveComponent>();
        while (query.MoveNext(out var sUid, out var slave))
        {
            if (slave.Master == uid && _mobState.IsAlive(sUid)) count++;
        }
        component.CurrentSlaves = count;

        bool isAscended = HasComp<ShadowlingAnnihilationComponent>(uid);

        // ВОЗНЕСЕНИЕ
        if (TryComp<ShadowlingAscendanceComponent>(uid, out var asc))
        {
            if (count >= asc.RequiredSlaves && !isAscended)
            {
                if (asc.ActionAscendanceEntity == null)
                    _actions.AddAction(uid, ref asc.ActionAscendanceEntity, asc.ActionAscendance);
            }
            else if (asc.ActionAscendanceEntity != null)
            {
                _actions.RemoveAction(uid, asc.ActionAscendanceEntity);
                asc.ActionAscendanceEntity = null;
            }
        }

        // КРИК
        if (TryComp<ShadowlingScreechComponent>(uid, out var screech))
        {
            if (isAscended || count >= screech.RequiredSlaves)
            {
                if (screech.ActionScreechEntity == null)
                    _actions.AddAction(uid, ref screech.ActionScreechEntity, screech.ActionScreech);
            }
            else if (screech.ActionScreechEntity != null)
            {
                _actions.RemoveAction(uid, screech.ActionScreechEntity);
                screech.ActionScreechEntity = null;
            }
        }

        // ВЕНЫ
        if (TryComp<ShadowlingFreezingVeinsComponent>(uid, out var veins))
        {
            if (isAscended || count >= veins.RequiredSlaves)
            {
                if (veins.ActionFreezingVeinsEntity == null)
                    _actions.AddAction(uid, ref veins.ActionFreezingVeinsEntity, veins.ActionFreezingVeins);
            }
            else if (veins.ActionFreezingVeinsEntity != null)
            {
                _actions.RemoveAction(uid, veins.ActionFreezingVeinsEntity);
                veins.ActionFreezingVeinsEntity = null;
            }
        }

        // МЕДИЦИНА
        if (TryComp<ShadowlingBlackMedComponent>(uid, out var med))
        {
            if (isAscended || count >= med.RequiredSlaves)
            {
                if (med.ActionBlackMedEntity == null)
                    _actions.AddAction(uid, ref med.ActionBlackMedEntity, med.ActionBlackMed);
            }
            else if (med.ActionBlackMedEntity != null)
            {
                _actions.RemoveAction(uid, med.ActionBlackMedEntity);
                med.ActionBlackMedEntity = null;
            }
        }
    }
}
