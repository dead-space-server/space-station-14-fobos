// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class MedPostMortemResistanceSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.MedPostMortemResistance;
    protected override ProtoId<VirusSymptomPrototype> PrototypeId => "MedPostMortemResistanceSymptom";
    private float _addDamageWhenDead = 2f;

    public MedPostMortemResistanceSymptom(TimedWindow effectTimedWindow) : base(effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);
    }

    public override void OnRemoved(EntityUid host, VirusComponent virus)
    {
        base.OnRemoved(host, virus);
    }

    public override void OnUpdate(EntityUid host, VirusComponent virus)
    {
        base.OnUpdate(host, virus);
    }

    public override void DoEffect(EntityUid host, VirusComponent virus)
    {

    }

    public override IVirusSymptom Clone()
    {
        return new MedPostMortemResistanceSymptom(EffectTimedWindow.Clone());
    }

    public override void ApplyDataEffect(VirusData data, bool add)
    {
        base.ApplyDataEffect(data, add);
        if (add)
            data.DamageWhenDead -= _addDamageWhenDead;
        else
            data.DamageWhenDead += _addDamageWhenDead;
    }
}
