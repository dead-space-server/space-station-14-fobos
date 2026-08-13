// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DeadSpace.SignatureOnPaper.Components;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.SignatureOnPaper;

public sealed partial class SignaturePaperSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignaturePaperComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<SignaturePaperComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, SignaturePaperComponent component, ExaminedEvent args)
    {
        if (component.NumberSignatures > 0)
            args.PushMarkup(Loc.GetString("signature-examined"));
    }

    private void OnGetVerbs(EntityUid uid, SignaturePaperComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (_hands.GetActiveItem(args.User) is not { } item)
            return;

        if (!CanSign(uid, component, item, args.User, quiet: true))
            return;

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("signature-verb-sign"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/_DeadSpace/Interface/VerbIcons/pen.svg.192dpi.png")),
            Act = () => TrySign(uid, component, item, args.User),
            Impact = LogImpact.Low
        });
    }

    public bool TrySign(
        EntityUid uid,
        SignaturePaperComponent? component = null,
        EntityUid? item = null,
        EntityUid? user = null,
        SignatureToolComponent? tool = null,
        PaperComponent? paper = null)
    {
        if (user == null || item == null)
            return false;

        if (!CanSign(uid, component, item.Value, user.Value, tool, paper))
            return false;

        if (!Resolve(uid, ref component, ref paper) || !Resolve(item.Value, ref tool))
            return false;

        Sign((uid, paper), component, tool, user.Value);
        return true;
    }

    public bool CanSign(
        EntityUid uid,
        SignaturePaperComponent? component = null,
        EntityUid? item = null,
        EntityUid? user = null,
        SignatureToolComponent? tool = null,
        PaperComponent? paper = null,
        bool quiet = false)
    {
        if (user == null || item == null)
            return false;

        if (!Resolve(uid, ref component, ref paper, false))
            return false;

        if (!Resolve(item.Value, ref tool, false))
            return false;

        if (component.NumberSignatures >= component.MaximumSignatures)
            return false;

        var name = Name(user.Value);
        if (paper.Signatures.Contains(name))
            return false;

        return true;
    }

    public void Sign(
        Entity<PaperComponent> entity,
        SignaturePaperComponent component,
        SignatureToolComponent tool,
        EntityUid user)
    {
        var name = Name(user);
        if (entity.Comp.Signatures.Contains(name) || component.NumberSignatures >= component.MaximumSignatures)
            return;

        entity.Comp.Signatures.Add(name);
        component.NumberSignatures += 1;
        Dirty(entity);
        Dirty(entity.Owner, component);

        if (tool.Sound != null)
            _audio.PlayPredicted(tool.Sound, entity, user);

        if (TryComp<AppearanceComponent>(entity, out var appearance))
            _appearance.SetData(entity, PaperComponent.PaperVisuals.Status, PaperComponent.PaperStatus.Written, appearance);

        if (_net.IsServer)
        {
            _adminLogger.Add(LogType.Action,
                LogImpact.Low,
                $"{ToPrettyString(user):player} signed {ToPrettyString(entity):entity}");
        }
    }
}
