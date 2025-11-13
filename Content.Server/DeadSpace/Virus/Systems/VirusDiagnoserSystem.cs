// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Content.Shared.Examine;
using Robust.Shared.Containers;
using Content.Server.DeadSpace.Virus.Components;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed class VirusDiagnoserSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    private const string ContainerKey = "virus-diagnoser";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VirusDiagnoserComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<VirusDiagnoserComponent, ExaminedEvent>(OnAfterInteract);
    }

    private void OnExamine(EntityUid uid, VirusDiagnoserComponent component, ExaminedEvent args)
    {
        if (!_container.TryGetContainer(uid, ContainerKey, out var container))
            return;

        if (container is ContainerSlot slot)
        {
            if (slot.ContainedEntity != null)
                args.PushMarkup(Loc.GetString("dna-material-attached"));
        }
    }

    private void OnAfterInteract(EntityUid uid, VirusDiagnoserComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { } target)
            return;

        if (!_container.TryGetContainer(target, "lock-container", out var lockContainer))
        {
            _popup.PopupEntity(Loc.GetString("medieval-add-lock-fail2"), args.User, args.User);
            return;
        }

        if (lockContainer is ContainerSlot slot)
        {
            if (slot.ContainedEntity != null)
            {
                _popup.PopupEntity(Loc.GetString("medieval-add-lock-fail1"), args.User, args.User);
                return;
            }

            if (!_container.Insert(uid, lockContainer))
            {
                _popup.PopupEntity(Loc.GetString("medieval-add-lock-fail3"), args.User, args.User);

                EnsureComp<MedievalLockableComponent>(target);

                return;
            }

            if (component.AttachSound != null)
                _audio.PlayPvs(component.AttachSound, Transform(target).Coordinates);

            _popup.PopupEntity(Loc.GetString("medieval-add-lock-pass"), args.User, args.User);
        }

    }

}