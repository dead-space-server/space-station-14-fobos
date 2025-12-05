// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Random;
using Content.Shared.DeadSpace.Languages.Prototypes;
using Content.Shared.DeadSpace.Languages.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Content.Shared.DeadSpace.Languages;
using Robust.Server.Player;
using Content.Shared.Chat;
using System.Linq;
using Content.Shared.Polymorph;
using Robust.Shared.GameStates;

namespace Content.Server.DeadSpace.Languages;

public sealed class LanguageSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public static readonly ProtoId<LanguagePrototype> DefaultLanguageId = "GeneralLanguage";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageComponent, ComponentGetState>(OnGetState);

        SubscribeLocalEvent<LanguageComponent, PolymorphedEvent>(OnPolymorphed);

        SubscribeAllEvent<SelectLanguageEvent>(OnSelectLanguage);
    }

    private void OnSelectLanguage(SelectLanguageEvent msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession.AttachedEntity;

        if (!player.HasValue)
            return;

        if (TryComp<LanguageComponent>(player, out var language))
            language.SelectedLanguage = msg.PrototypeId;
    }

    private void OnGetState(EntityUid uid, LanguageComponent component, ref ComponentGetState args)
    {
        args.State = new LanguageComponentState(component.KnownLanguages, component.CantSpeakLanguages);
    }

    private void OnPolymorphed(EntityUid uid, LanguageComponent component, PolymorphedEvent args)
    {
        var lang = EnsureComp<LanguageComponent>(args.NewEntity);
        lang.CopyFrom(component);
    }

    public string ReplaceWordsWithLexicon(string message, ProtoId<LanguagePrototype> languageId)
    {
        if (String.IsNullOrEmpty(languageId))
            return message;

        if (!_prototypeManager.TryIndex(languageId, out var languageProto))
            return message;

        var lexiconWords = languageProto.Lexicon;

        if (lexiconWords == null || lexiconWords.Count == 0)
            return message;

        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(words[i]))
            {
                var randIndex = _random.Next(lexiconWords.Count);
                words[i] = lexiconWords[randIndex];
            }
        }
        return string.Join(' ', words);
    }

    public string GetLangName(ProtoId<LanguagePrototype>? languageId)
    {
        var name = "Неизвестно";

        if (String.IsNullOrEmpty(languageId))
            return name;

        if (_prototypeManager.TryIndex(languageId, out var languageProto))
            name = languageProto.Name;

        return name;
    }

    public string GetLangName(EntityUid uid, LanguageComponent? component = null)
    {
        var name = "Неизвестно";

        if (!Resolve(uid, ref component, false))
            return name;

        if (String.IsNullOrEmpty(component.SelectedLanguage))
            return name;

        if (_prototypeManager.TryIndex<LanguagePrototype>(component.SelectedLanguage, out var languageProto))
            name = languageProto.Name;

        return name;
    }

    public HashSet<ProtoId<LanguagePrototype>>? GetKnownLanguages(EntityUid entity)
    {
        if (!TryComp<LanguageComponent>(entity, out var component))
            return null;

        return component.KnownLanguages;
    }

    public bool KnowsLanguage(EntityUid receiver, ProtoId<LanguagePrototype> senderLanguageId)
    {
        var languages = GetKnownLanguages(receiver);

        if (languages == null) // если нет языков, значит знает всё
            return true;

        return languages.Contains(senderLanguageId);
    }

    public void AddKnowLanguage(EntityUid uid, ProtoId<LanguagePrototype> languageId, LanguageComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        component.KnownLanguages.Add(languageId);
        Dirty(uid, component);
    }

    public bool NeedGenerateTTS(EntityUid sourceUid, ProtoId<LanguagePrototype> prototypeId, bool isWhisper)
    {
        if (String.IsNullOrEmpty(prototypeId))
            return false;

        if (!_prototypeManager.TryIndex(prototypeId, out var languageProto))
            return false;

        if (!languageProto.GenerateTTSForLexicon)
            return false;

        float range = isWhisper ? SharedChatSystem.WhisperMuffledRange : SharedChatSystem.VoiceRange;

        var ents = _lookup.GetEntitiesInRange<ActorComponent>(_transform.GetMapCoordinates(sourceUid, Transform(sourceUid)), range).ToList();

        var hasListener = ents.Any(ent =>
            ent.Comp.PlayerSession is { AttachedEntity: not null }
            && !KnowsLanguage(ent.Owner, prototypeId));

        return hasListener;
    }

    public bool NeedGenerateDirectTTS(EntityUid uid, ProtoId<LanguagePrototype> prototypeId)
    {
        if (String.IsNullOrEmpty(prototypeId))
            return false;

        if (!_prototypeManager.TryIndex(prototypeId, out var languageProto))
            return false;

        if (!languageProto.GenerateTTSForLexicon)
            return false;

        if (KnowsLanguage(uid, prototypeId))
            return false;

        return true;
    }

    public bool NeedGenerateGlobalTTS(ProtoId<LanguagePrototype> prototypeId, out List<ICommonSession> understandings)
    {
        understandings = GetUnderstanding(prototypeId);

        if (String.IsNullOrEmpty(prototypeId))
            return false;

        if (!_prototypeManager.TryIndex(prototypeId, out var languageProto))
            return false;

        if (!languageProto.GenerateTTSForLexicon)
            return false;

        if (understandings.Count <= 0)
            return false;

        return true;
    }

    public bool NeedGenerateRadioTTS(ProtoId<LanguagePrototype> prototypeId, EntityUid[] recivers, out List<EntityUid> understandings, out List<EntityUid> notUnderstandings)
    {
        understandings = new List<EntityUid>();
        notUnderstandings = new List<EntityUid>();
        bool result = false;

        foreach (var uid in recivers)
        {
            if (!KnowsLanguage(uid, prototypeId))
            {
                notUnderstandings.Add(uid);
                result = true;
            }
            else
            {
                understandings.Add(uid);
            }
        }

        return result;
    }

    public List<ICommonSession> GetUnderstanding(ProtoId<LanguagePrototype> languageId)
    {
        var understanding = new List<ICommonSession>();

        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity == null)
            {
                understanding.Add(session);
                continue;
            }

            if (KnowsLanguage(session.AttachedEntity.Value, languageId))
                understanding.Add(session);
        }

        return understanding;
    }
}
