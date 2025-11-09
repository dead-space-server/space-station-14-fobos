// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

namespace Content.Server.DeadSpace.Virus.Symptoms;

public enum VirusSymptom
{
    // Базовые симптомы
    Cough,                // кашель, увеличивает распространение
    Fever,                // повышает температуру тела, наносит лёгкий урон
    Vomit,                // тошнота, повышает уровень распространения
    Rash,                 // повышает уровень распространения, персонаж чешется

    // Средние симптомы
    Drowsiness,           // впадание в сон

    // Продвинутые / “тяжёлые”
    Necrosis,             // медленно убивает тело
    Zombification,        // при смерти носителя превращает в зомби
}