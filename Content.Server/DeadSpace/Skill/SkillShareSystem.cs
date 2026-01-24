// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Administration.Managers;
using Content.Server.DeadSpace.Skill;
using Content.Server.EUI;
using Content.Server.Mind;
using Content.Shared.DeadSpace.Skills;
using Content.Shared.DeadSpace.Skills.Components;
using Content.Shared.Ghost;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.DeadSpace.Skill;

public sealed class SkillShareSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SkillSystem _skillSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private const float MaxShareDistance = 3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SkillComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<SkillComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (args.User == args.Target)
            return;

        var user = args.User;
        var target = entity.Owner;

        var isAdminGhost = false;
        if (HasComp<GhostComponent>(args.User) &&
            _playerManager.TryGetSessionByEntity(args.User, out var userSession) &&
            _adminManager.IsAdmin(userSession))
        {
            isAdminGhost = true;
        }

        // For non-admin users, apply regular restrictions
        if (!isAdminGhost)
        {
            if (HasComp<GhostComponent>(args.User))
                return;

            if (!_mindSystem.TryGetMind(args.User, out _, out _))
                return;

            if (!HasComp<SkillComponent>(args.User))
                return;

            if (!args.CanInteract || !args.CanAccess)
                return;

            var verb = new Verb
            {
                Text = Loc.GetString("skill-share-verb-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/information.svg.192dpi.png")),
                Act = () => RequestSkillShare(user, target),
                Priority = -1
            };

            args.Verbs.Add(verb);
        }
        else
        {
            var adminVerb = new Verb
            {
                Text = Loc.GetString("skill-share-verb-admin-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/information.svg.192dpi.png")),
                Act = () => OpenSkillsDirectly(user, target),
                Priority = -1
            };

            args.Verbs.Add(adminVerb);
        }
    }

    private void RequestSkillShare(EntityUid requester, EntityUid target)
    {
        var requesterXform = Transform(requester);
        var targetXform = Transform(target);

        if (!requesterXform.Coordinates.TryDistance(EntityManager, _transformSystem, targetXform.Coordinates, out var distance))
            return;

        if (distance > MaxShareDistance)
            return;

        if (!_playerManager.TryGetSessionByEntity(target, out var targetSession))
            return;

        var requesterName = MetaData(requester).EntityName;

        _euiManager.OpenEui(new SkillShareRequestEui(target, requester, this, requesterName), targetSession);
    }

    private void OpenSkillsDirectly(EntityUid viewer, EntityUid target)
    {
        if (!_playerManager.TryGetSessionByEntity(viewer, out var viewerSession))
            return;

        if (!TryComp<SkillComponent>(target, out var skillComponent))
            return;

        var skills = new List<SkillInfo>();
        foreach (var skill in skillComponent.Skills)
        {
            var info = _skillSystem.GetSkillInfo(target, skill.Key);
            if (info != null)
                skills.Add(info.Value);
        }

        var targetName = MetaData(target).EntityName;

        _euiManager.OpenEui(new SkillsListEui(targetName, skills), viewerSession);
    }

    public void HandleSkillShareResponse(EntityUid target, EntityUid requester, ICommonSession targetSession, bool accepted)
    {
        if (!accepted)
            return;

        if (!EntityManager.EntityExists(requester))
            return;

        var requesterXform = Transform(requester);
        var targetXform = Transform(target);

        if (!requesterXform.Coordinates.TryDistance(EntityManager, _transformSystem, targetXform.Coordinates, out var distance))
            return;

        if (distance > MaxShareDistance)
            return;

        if (!_playerManager.TryGetSessionByEntity(requester, out var requesterSession))
            return;

        if (!TryComp<SkillComponent>(target, out var skillComponent))
            return;

        var skills = new List<SkillInfo>();
        foreach (var skill in skillComponent.Skills)
        {
            var info = _skillSystem.GetSkillInfo(target, skill.Key);
            if (info != null)
                skills.Add(info.Value);
        }

        var targetName = MetaData(target).EntityName;

        _euiManager.OpenEui(new SkillsListEui(targetName, skills), requesterSession);
    }
}
