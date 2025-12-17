// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Chat.Systems;
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Server.DeadSpace.Virus.Systems;
using Content.Shared.DeadSpace.TimeWindow;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class RashSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.Rash;
    protected override float AddInfectivity => 0.1f;
    private const string RashEmote = "чешется";

    public RashSymptom(IEntityManager entityManager, IGameTiming timing, IRobustRandom random, TimedWindow effectTimedWindow) : base(entityManager, timing, random, effectTimedWindow)
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
        var chatSystem = EntityManager.System<ChatSystem>();
        var virusSystem = EntityManager.System<VirusSystem>();

        chatSystem.TrySendInGameICMessage(host,
                            RashEmote,
                            InGameICChatType.Emote,
                            ChatTransmitRange.Normal);
    }

    public override IVirusSymptom Clone()
    {
        return new RashSymptom(EntityManager, Timing, Random, CloneTimedWindow());
    }
}
