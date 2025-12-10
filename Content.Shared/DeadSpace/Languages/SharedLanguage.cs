// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Serialization;
using Content.Shared.DeadSpace.Languages.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.Languages;

public enum SpeechMode
{
    Lexicon,
    Syllable,
    Alphabet,
    Pattern
}


[Serializable, NetSerializable]
public sealed partial class SelectLanguageEvent : EntityEventArgs
{
    public int Target { get; }
    public ProtoId<LanguagePrototype> PrototypeId { get; }

    public SelectLanguageEvent(int target, ProtoId<LanguagePrototype> prototypeId)
    {
        Target = target;
        PrototypeId = prototypeId;
    }
}
