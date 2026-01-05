using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Medieval.Skills;

/// <summary>
/// Name - название навыка.
/// Description - описание навыка.
/// Progress - прогресс изучения навыка.
/// </summary>
[Serializable, NetSerializable]
public record struct SkillInfo(string Name, string Description, SpriteSpecifier Icon, float Progress);
