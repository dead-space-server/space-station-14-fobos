# DS14-start
# ВорПРО — the thief's PDA program

thief-program-name = ВорПРО
thief-program-unlocked = Программа «ВорПРО» установлена на ваш КПК. Проверьте список программ!

## Header
thief-program-balance = Баланс: [color=#8fa063]{$balance}[/color] dCR
thief-program-goal = Цель раунда: [color=#c9a227]{$earned} / {$target}[/color] dCR при себе ({$percent}%)

## Tabs
thief-program-tab-requests = Запросы
thief-program-tab-uplink = Аплинк

## Requests tab
thief-program-section-active = Принятые запросы
thief-program-section-offers = Доступные запросы
thief-program-hint-no-beacon = [color=#c9a227]Установите и разверните воровской маяк, чтобы продавать товары.[/color]
thief-program-request-offer =
    {$name} ×{$count} — [color=#8fa063]{$price} dCR[/color] • срок: {$minutes} мин.
thief-program-request-active =
    {$name} ×{$count} — [color=#8fa063]{$price} dCR[/color] • осталось: {$minutes}:{$seconds}
    [color=#ff4d4d]Просрочено![/color] Цена снижена на 15%.
    Отнесите товар к воровскому маяку и нажмите «Продать».
thief-program-request-expired =
    {$name} ×{$count} — [color=#a86f32]~{$price} dCR[/color] • [color=#ff4d4d]просрочен[/color]
    Отнесите товар к воровскому маяку и нажмите «Продать».
thief-program-accept = Взять
thief-program-sell = Продать
thief-program-decline-tooltip = Отказаться от запроса

## Uplink tab
thief-program-uplink-cost = {$cost} dCR
thief-program-buy = Купить
thief-program-exchange-placeholder = Сумма для отмывания...
thief-program-exchange-button = Отмыть 1:1

## Categories
thief-program-category-tools = Инструменты
thief-program-category-gear = Снаряжение
thief-program-category-implants = Импланты
thief-program-category-misc = Разное
thief-program-category-sets = Наборы

## Server popups
thief-program-requests-limit = Слишком много активных запросов. Сначала выполните или отклоните текущие.
thief-program-error-no-mind = Программа не может определить владельца.
thief-program-error-no-beacon = Не найден привязанный воровской маяк!
thief-program-error-too-far = Вы слишком далеко от своего маяка.
thief-program-error-not-enough = Рядом с маяком недостаточно подходящих товаров.
thief-program-uplink-error = Такой товар отсутствует в каталоге.
thief-program-uplink-no-money = Недостаточно грязных кредитов.
thief-program-exchange-invalid = Укажите положительную сумму.
thief-program-exchange-not-enough = Недостаточно грязных кредитов для обмена.
thief-program-sold-in-time = Сделка завершена вовремя! Получено: {$amount} dCR (бонус +15%).
thief-program-sold-late = Сделка завершена с опозданием. Получено: {$amount} dCR (−15%).
thief-program-exchanged = Обмен выполнен: {$amount} обычных кредитов зачислено.
thief-program-carry-hint = Важно: в цель раунда идут только dCR, которые вы несёте с собой!

## Uplink listings
thief-program-listing-beacon-name = Воровской маяк (запасной)
thief-program-listing-beacon-desc = Ещё один передатчик сигналов. Без него не продавать товары.
thief-program-listing-c4-name = Заряд C4
thief-program-listing-c4-desc = Быстро и громко решает проблемы с запертыми дверями и стенами.
thief-program-listing-emag-name = Эмаг-карта
thief-program-listing-emag-desc = Взламывает замки, консоли и многое другое. Классика жанра.
thief-program-listing-access-breaker-name = Взломщик доступа
thief-program-listing-access-breaker-desc = Тихо вскрывает двери со шлюзами.
thief-program-listing-radio-jammer-name = Радиоглушитель
thief-program-listing-radio-jammer-desc = Глушит связь экипажа на время вашего визита.
thief-program-listing-chameleon-projector-name = Хамелеон-проектор
thief-program-listing-chameleon-projector-desc = Создаёт иллюзорную копию любого предмета.
thief-program-listing-fake-mindshield-name = Фальшивый имплант щита разума
thief-program-listing-fake-mindshield-desc = Показания сканера обмануты: вы выглядите чистым перед законом.
thief-program-listing-voice-mask-name = Имплант маскировки голоса
thief-program-listing-voice-mask-desc = Позволяет говорить чужим голосом.
thief-program-listing-storage-implant-name = Вместительный имплант
thief-program-listing-storage-implant-desc = Карман внутри вас самих. Идеален для незаметного выноса ценностей.
thief-program-listing-dna-scrambler-name = Имплант перемены ДНК
thief-program-listing-dna-scrambler-desc = Мгновенно меняет вашу внешность и личность.
thief-program-listing-nocturine-name = Флаконы ноктюрина
thief-program-listing-nocturine-desc = Снотворное прямого действия. Жертва даже не проснётся.
thief-program-listing-hypopen-box-name = Коробка гипопенов
thief-program-listing-hypopen-box-desc = Шприц-ручка для тихого усыпления цели.
thief-program-listing-cyber-pen-name = Кибер-ручка
thief-program-listing-cyber-pen-desc = Ручка со скрытыми функциями для настоящего профессионала.
thief-program-listing-station-master-name = Ключ шифрования "Мастер станции"
thief-program-listing-station-master-desc = Доступ ко всем каналам связи станции.
thief-program-listing-smoke-name = Дымовые гранаты
thief-program-listing-smoke-desc = Три гранаты для эффектного исчезновения.
thief-program-listing-fulton-name = Набор фултонов
thief-program-listing-fulton-desc = Маяк и два фултона для подъёма грузов на орбиту.
thief-program-listing-invisible-crate-name = Невидимый ящик
thief-program-listing-invisible-crate-desc = Ящик, который почти невозможно заметить. Для тайников.
# DS14-end
