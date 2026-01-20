using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Skills;

/// <summary>
/// Name - название навыка.
/// Description - описание навыка.
/// Progress - прогресс изучения навыка.
/// IconSize - размер иконки навыка.
/// </summary>
[Serializable, NetSerializable]
public record struct SkillInfo(string Name, string Description, SpriteSpecifier Icon, float Progress, int IconSize);
