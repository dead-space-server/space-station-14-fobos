// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.CustomNameOnSpawn;

[RegisterComponent]
public sealed partial class CustomNameOnSpawnComponent : Component
{
    /// <summary>
    /// Обязательный префикс имени. Игрок вводит только личную часть,
    /// итоговое имя будет: "{NamePrefix} {введённое}".
    /// Если null, то игрок вводит полное имя сам.
    /// </summary>
    [DataField]
    public string? NamePrefix;
}