// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Body.Systems;
using Content.Shared.DragDrop;
using Content.Shared.Interaction.Events;

namespace Content.Shared.DeadSpace.Carrying;

public sealed class SharedCarryInteractionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CarriedComponent, GettingInteractedWithAttemptEvent>(OnGettingInteractedWithAttempt);
        SubscribeLocalEvent<CarriedComponent, CanDragEvent>(OnCanDrag, after: [typeof(SharedBodySystem)]);
    }

    private void OnGettingInteractedWithAttempt(Entity<CarriedComponent> ent, ref GettingInteractedWithAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnCanDrag(Entity<CarriedComponent> ent, ref CanDragEvent args)
    {
        args.Handled = false;
    }
}
