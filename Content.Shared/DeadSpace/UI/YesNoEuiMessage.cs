// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.UI
{
    [Serializable, NetSerializable]
    public enum YesNoUiButton
    {
        No,
        Yes,
    }

    [Serializable, NetSerializable]
    public sealed class YesNoChoiceMessage : EuiMessageBase
    {
        public readonly YesNoUiButton Button;

        public YesNoChoiceMessage(YesNoUiButton button)
        {
            Button = button;
        }
    }

    [Serializable, NetSerializable]
    public sealed class YesNoEuiState : EuiStateBase
    {
        public string Title = string.Empty;
        public string Text = string.Empty;

        public YesNoEuiState(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}
