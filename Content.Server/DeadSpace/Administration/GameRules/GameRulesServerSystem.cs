// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.DeadSpace.Administration.GameRules;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.Administration.GameRules;

public sealed class GameRulesServerSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly Dictionary<string, string> _addedByAdmin = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestGameRulesListMessage>(OnRequestGameRulesList);
        SubscribeNetworkEvent<AddGameRuleRequestMessage>(OnAddGameRuleRequest);
    }

    private void OnAddGameRuleRequest(AddGameRuleRequestMessage msg, EntitySessionEventArgs args)
    {
        if (!_prototypeManager.HasIndex<EntityPrototype>(msg.RuleId))
            return;

        _ticker.AddGameRule(msg.RuleId);
        _addedByAdmin[msg.RuleId] = msg.AdminName;
    }

    private void OnRequestGameRulesList(RequestGameRulesListMessage msg, EntitySessionEventArgs args)
    {
        var allRules = _ticker.AllPreviousGameRules;
        var entries = new List<RuleEntry>();

        if (allRules.Count > 0)
        {
            var sorted = allRules.OrderBy(rule => rule.Item1).ToList();
            foreach (var (time, rule) in sorted)
            {
                var cleanRule = rule.EndsWith(" (Pending)") ? rule[..^9].Trim() : rule.Trim();
                var admin = _addedByAdmin.GetValueOrDefault(cleanRule);
                entries.Add(new RuleEntry(time, rule, admin));
            }
        }

        var roundActive = _ticker.RunLevel == GameRunLevel.InRound;
        var roundDuration = roundActive ? _ticker.RoundDuration() : TimeSpan.Zero;

        var response = new GameRulesListResponseMessage(entries, roundDuration, roundActive);
        RaiseNetworkEvent(response, args.SenderSession);
    }

    public void ReportRuleAddedByAdmin(string ruleId, string? adminName)
    {
        if (adminName != null)
            _addedByAdmin[ruleId] = adminName;
    }
}