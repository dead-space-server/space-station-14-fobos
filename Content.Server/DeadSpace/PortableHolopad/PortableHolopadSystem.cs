// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Holopad;
using Content.Shared.Speech;
using Content.Shared.Telephone;
using Content.Shared.Verbs;
using Content.Server.Popups;
using Content.Server.Telephone;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server.Holopad;

public sealed class PortableHolopadSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly TransformSystem _xformSystem = default!;
    [Dependency] private readonly TelephoneSystem _telephoneSystem = default!;
    [Dependency] private readonly HolopadSystem _holopadSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<PortableHolopadComponent, GetVerbsEvent<AlternativeVerb>>(AddDeployVerb);
        SubscribeLocalEvent<PortableHolopadComponent, TelephoneMessageSentEvent>(OnTelephoneMessageSent);
    }

    private void AddDeployVerb(Entity<PortableHolopadComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;

        AlternativeVerb verb = new()
        {
            Act = () => ToggleDeployed(entity, user),
            Text = entity.Comp.Deployed ? "Собрать голопад." : "Развернуть голопад.",
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/fold.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void OnTelephoneMessageSent(Entity<PortableHolopadComponent> entity, ref TelephoneMessageSentEvent args)
    {
        if (!entity.Comp.Deployed)
        {
            if (TryComp<SpeechComponent>(entity, out var holopadSpeech) &&
                TryComp<TelephoneComponent>(entity, out var telephone))
            {
                _telephoneSystem.SetSpeakerForTelephone((entity, telephone), (entity, holopadSpeech));
            }
        }
    }

    public void ToggleDeployed(Entity<PortableHolopadComponent> entity, EntityUid user)
    {
        if (_container.IsEntityInContainer(entity))
        {
            _popupSystem.PopupEntity("Голопад должен быть на полу!", entity, user);
            return;
        }

        if (!TryComp<HolopadComponent>(entity, out var holopad) || 
            !TryComp<TelephoneComponent>(entity, out var telephone))
            return;

        entity.Comp.Deployed = !entity.Comp.Deployed;

        if (entity.Comp.Deployed)
        {
            if (_telephoneSystem.IsTelephoneEngaged((entity, telephone)))
                _holopadSystem.GenerateHologram((entity, holopad));

            _xformSystem.AnchorEntity(entity);
            _popupSystem.PopupEntity("Голопад развернут. Теперь используйте его для запуска вызова.", entity, user);
        }
        else
        {
            _xformSystem.Unanchor(entity);

            if (holopad.Hologram != null)
                _holopadSystem.DeleteHologram(holopad.Hologram.Value, (entity, holopad));

            if (TryComp<SpeechComponent>(entity, out var speech))
                _telephoneSystem.SetSpeakerForTelephone((entity, telephone), (entity, speech));

            _popupSystem.PopupEntity("Голопад собран.", entity, user);
        }

        Dirty(entity);
    }
}