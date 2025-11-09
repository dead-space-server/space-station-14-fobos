// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Chat.Systems;
using Content.Server.DeadSpace.Virus.Components;
using Content.Shared.Chat.Prototypes;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.Speech.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.Virus.Symptoms;

public sealed class CoughSymptom : VirusSymptomBase
{
    public override VirusSymptom Type => VirusSymptom.Cough;
    protected override float AddInfectivity => 0.1f;
    private static readonly ProtoId<EmotePrototype> CoughEmote = "Cough";

    public CoughSymptom(IEntityManager entityManager, IGameTiming timing, TimedWindow effectTimedWindow) : base(entityManager, timing, effectTimedWindow)
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
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        var chatSystem = EntityManager.System<ChatSystem>();

        if (EntityManager.TryGetComponent<VocalComponent>(host, out var vocal))
        {

            chatSystem.TryEmoteWithChat(host,
                            CoughEmote,
                            ChatTransmitRange.HideChat,
                            ignoreActionBlocker: true);

            if (vocal.EmoteSounds != null)
            {
                chatSystem.TryPlayEmoteSound(
                    host,
                    protoMan.Index(vocal.EmoteSounds),
                    CoughEmote
                );
            }
        }
    }

    public override IVirusSymptom Clone()
    {
        // Создаём новый TimedWindow с теми же параметрами
        var newTimedWindow = new TimedWindow(EffectTimedWindow.MinSeconds, EffectTimedWindow.MaxSeconds, EffectTimedWindow.Timing, EffectTimedWindow.Random);

        return new CoughSymptom(EntityManager, Timing, newTimedWindow);
    }
}
