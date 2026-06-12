using Content.Shared.DeadSpace.Radio.Systems;
using Content.Shared.DeadSpace.Radio.Components;
using Content.Shared.Radio.EntitySystems;

namespace Content.Server.DeadSpace.Radio.Systems;

public sealed class RadioToggleActionSystem : SharedRadioToggleActionSystem
{
    [Dependency] private readonly SharedRadioDeviceSystem _radioDevice = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioToggleActionComponent, RadioToggleEvent>(OnRadioToggle);
    }

    private void OnRadioToggle(Entity<RadioToggleActionComponent> ent, ref RadioToggleEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Enabled = !ent.Comp.Enabled;
        args.Handled = true;

        _radioDevice.SetMicrophoneEnabled(ent, args.Performer, ent.Comp.Enabled);
        _radioDevice.SetSpeakerEnabled(ent, args.Performer, ent.Comp.Enabled);
        Dirty(ent);
    }
}