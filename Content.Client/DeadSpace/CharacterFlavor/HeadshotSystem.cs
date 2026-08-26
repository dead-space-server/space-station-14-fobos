using Content.Shared.DeadSpace.CharacterFlavor;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client.DeadSpace.CharacterFlavor;

public sealed class HeadshotSystem : SharedHeadshotSystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<HeadshotDownloadResultEvent>(OnHeadshotDownloadResult);
        SubscribeNetworkEvent<HeadshotExamineResultEvent>(OnHeadshotExamineResult);
    }

    protected override void OpenHeadshotFlavor(EntityUid actor, EntityUid target)
    {
        base.OpenHeadshotFlavor(actor, target);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (!HasComp<HeadshotComponent>(target))
            return;

        var controller = _ui.GetUIController<HeadshotUIController>();
        controller.OpenExamineWindow(target);
    }

    private void OnHeadshotDownloadResult(HeadshotDownloadResultEvent ev)
    {
        var controller = _ui.GetUIController<HeadshotUIController>();
        controller.OnHeadshotDownloadResult(ev);
    }

    private void OnHeadshotExamineResult(HeadshotExamineResultEvent ev)
    {
        var controller = _ui.GetUIController<HeadshotUIController>();
        controller.OnHeadshotExamineResult(ev);
    }
}
