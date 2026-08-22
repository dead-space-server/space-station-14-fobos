using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Shared.Humanoid;
using Content.Shared.Roles.Components;
using Robust.Shared.Localization;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Rules;

public sealed class ThiefRuleSystem : GameRuleSystem<ThiefRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    // DS14-start
    /// <summary>
    /// How many adjectives-dataset-N locale keys to scan when collecting code word candidates.
    /// </summary>
    private const int MaxAdjectiveKeys = 2000;

    /// <summary>
    /// Fallback words in case the adjectives dataset is missing entirely.
    /// </summary>
    private static readonly string[] FallbackCodeWords =
    {
        "ТАЙНЫЙ", "ГРЯЗНЫЙ", "НОЧНОЙ", "КРИМИНАЛЬНЫЙ", "ТИХИЙ", "ЗОЛОТОЙ",
    };
    // DS14-end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThiefRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<ThiefRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    // Greeting upon thief activation
    private void AfterAntagSelected(Entity<ThiefRuleComponent> mindId, ref AfterAntagEntitySelectedEvent args)
    {
        var ent = args.EntityUid;

        // DS14: pick the round's code word for the ВорПРО program once per rule
        if (string.IsNullOrWhiteSpace(mindId.Comp.CodeWord))
            mindId.Comp.CodeWord = GenerateCodeWord();

        _antag.SendBriefing(ent, MakeBriefing(ent), null, null);
    }

    // Character screen briefing
    private void OnGetBriefing(Entity<ThiefRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;
        args.Append(MakeBriefing(ent.Value));
    }

    private string MakeBriefing(EntityUid ent)
    {
        var isHuman = HasComp<HumanoidAppearanceComponent>(ent);
        var briefing = isHuman
            ? Loc.GetString("thief-role-greeting-human")
            : Loc.GetString("thief-role-greeting-animal");

        if (isHuman)
            briefing += "\n \n" + Loc.GetString("thief-role-greeting-equipment") + "\n";

        // DS14: tell the thief about the goal and reveal the round's code word;
        // the word is also planted as a comment under a station news article.
        briefing += "\n" + Loc.GetString("thief-role-greeting-goal") + "\n";

        var codeWord = GetCodeWord();
        if (codeWord != null)
            briefing += "\n" + Loc.GetString("thief-role-greeting-codeword", ("codeword", codeWord)) + "\n";

        return briefing;
    }

    /// <summary>
    /// Returns the active rule's code word, generating it if necessary.
    /// </summary>
    private string? GetCodeWord()
    {
        foreach (var rule in _gameTicker.GetActiveGameRules())
        {
            if (!TryComp<ThiefRuleComponent>(rule, out var comp))
                continue;

            if (string.IsNullOrWhiteSpace(comp.CodeWord))
                comp.CodeWord = GenerateCodeWord();

            return comp.CodeWord;
        }

        return null;
    }

    #region Code word

    // DS14-start
    /// <summary>
    /// Builds the round's code word from a random adjective of the adjectives.ftl
    /// locale dataset, e.g. "тайный".
    /// </summary>
    private string GenerateCodeWord()
    {
        var adjectives = new List<string>();
        for (var i = 1; i <= MaxAdjectiveKeys; i++)
        {
            if (!_loc.TryGetString($"adjectives-dataset-{i}", out var word) ||
                string.IsNullOrWhiteSpace(word))
            {
                break;
            }

            adjectives.Add(word.Trim());
        }

        return adjectives.Count > 0 ? _random.Pick(adjectives) : _random.Pick(FallbackCodeWords);
    }
    // DS14-end

    #endregion
}
