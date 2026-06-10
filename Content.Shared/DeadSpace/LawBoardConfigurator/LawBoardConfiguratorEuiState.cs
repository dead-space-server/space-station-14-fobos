using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.LawBoardConfigurator;

[Serializable, NetSerializable]
public sealed class LawBoardConfiguratorEuiState : EuiStateBase
{
    public List<SiliconLaw> Laws { get; }
    public NetEntity Target { get; }
    public string BoardName { get; }
    public bool HasBoard { get; }

    public LawBoardConfiguratorEuiState(List<SiliconLaw> laws, NetEntity target, string boardName, bool hasBoard)
    {
        Laws = laws;
        Target = target;
        BoardName = boardName;
        HasBoard = hasBoard;
    }
}

[Serializable, NetSerializable]
public sealed class LawBoardConfiguratorSaveMessage : EuiMessageBase
{
    public List<SiliconLaw> Laws { get; }
    public NetEntity Target { get; }
    public string BoardName { get; }

    public LawBoardConfiguratorSaveMessage(List<SiliconLaw> laws, NetEntity target, string boardName)
    {
        Laws = laws;
        Target = target;
        BoardName = boardName;
    }
}

[Serializable, NetSerializable]
public sealed class LawBoardConfiguratorEjectMessage : EuiMessageBase
{
}
