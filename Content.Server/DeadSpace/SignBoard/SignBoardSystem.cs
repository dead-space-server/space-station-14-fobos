using Content.Shared.DeadSpace.SignBoard;
using Content.Shared.TextScreen;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server.DeadSpace.SignBoard;

public sealed class SignBoardSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SignBoardComponent, SignBoardSetTextMessage>(OnSetText);
        SubscribeLocalEvent<SignBoardComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnUIOpened(Entity<SignBoardComponent> entity, ref BoundUIOpenedEvent args)
    {
        _ui.SetUiState(entity.Owner, SignBoardUiKey.Key, new SignBoardBoundUserInterfaceState(entity.Comp.Text));
    }

    private void OnSetText(Entity<SignBoardComponent> entity, ref SignBoardSetTextMessage args)
    {
        entity.Comp.Text = args.Text;
        _appearance.SetData(entity.Owner, TextScreenVisuals.ScreenText, args.Text);
        _audio.PlayPvs(entity.Comp.Sound, entity.Owner);
        Dirty(entity);
    }
}
