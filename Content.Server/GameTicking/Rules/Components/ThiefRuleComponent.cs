using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Stores data for <see cref="ThiefRuleSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(ThiefRuleSystem))]
public sealed partial class ThiefRuleComponent : Component
{
    /// <summary>
    /// DS14: The code word that unlocks the ВорПРО program on the thief's PDA.
    /// Generated once per round from the adjectives.ftl locale dataset when the first
    /// thief is selected. The thief must write it into a news comment to activate the program.
    /// </summary>
    [DataField]
    public string CodeWord = string.Empty;
}
