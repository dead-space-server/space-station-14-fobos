using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared.DeadSpace.FakeDescription;

public sealed class FakeInsulatedSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FakeInsulatedComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
    }

    private void OnDetailedExamine(EntityUid ent, FakeInsulatedComponent component, ref GetVerbsEvent<ExamineVerb> args)
    {
        var iconTexture = "/Textures/Interface/VerbIcons/zap.svg.192dpi.png";

        _examine.AddHoverExamineVerb(args,
            component,
            Loc.GetString("insulated-examinable-verb-text"),
            Loc.GetString("insulated-examinable-verb-text-message"),
            iconTexture
        );
    }

}
