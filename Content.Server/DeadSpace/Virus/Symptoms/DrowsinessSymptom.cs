// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class DrowsinessSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.Drowsiness;
    protected override float AddInfectivity => 0.05f;
    private TimedWindow _slipDuration = default!;
    public static readonly EntProtoId StatusEffectForcedSleeping = "StatusEffectForcedSleeping";

    public DrowsinessSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);

        _slipDuration = new TimedWindow(5f, 15f, Timing, Random);
        EffectTimedWindow.AddTime(_slipDuration.Remaining);
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
        EffectTimedWindow.AddTime(_slipDuration.Remaining);

        var statusEffectsSystem = EntityManager.System<StatusEffectsSystem>();

        _slipDuration.Reset();
        var duration = Math.Abs(Timing.CurTime.TotalSeconds - _slipDuration.Remaining.TotalSeconds);

        statusEffectsSystem.TryAddStatusEffectDuration(host, StatusEffectForcedSleeping, TimeSpan.FromSeconds(duration));

        EffectTimedWindow.AddTime(_slipDuration.Remaining);
    }

    public override IVirusSymptom Clone()
    {
        return new DrowsinessSymptom(EntityManager, Timing, Random, CloneTimedWindow());
    }
}
