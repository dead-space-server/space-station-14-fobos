using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.QueueTerminal;

/// <summary>
/// Ключ данных внешнего вида, используемый как дисплеем терминала, так и билетом
/// Спрайт, определяющий, какой номер отображать
/// </summary>
[Serializable, NetSerializable]
public enum QueueDisplayVisuals : byte
{
    Number,
}
[Serializable, NetSerializable]
public enum QueueDigitLayers : byte
{
    Hundreds,
    Tens,
    Ones,
}
