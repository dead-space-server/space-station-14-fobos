// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Necromorphs.Unitology.Components;
using Content.Server.GameTicking.Rules;

namespace Content.Server.DeadSpace.Necromorphs.Unitology;

public sealed class UnitologyEnslavedSystem : EntitySystem
{
    [Dependency] private readonly UnitologyRuleSystem _unitologyRule = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnitologyEnslavedComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, UnitologyEnslavedComponent comp, ComponentInit args)
    {
        _unitologyRule.TryGrantUnitologyRole(uid, UnitologyRuleSystem.EnslavedUnitologyAntagRole);
    }
}
