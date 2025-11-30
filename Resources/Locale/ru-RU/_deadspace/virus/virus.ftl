virus-resistance-coefficient-value = - Шанс [color=violet]заражения вирусом[/color] снижен на [color=purple]{ $value }%[/color].

# При получении урона от некроза
virus-necrosis-popup-1 = Ты чувствуешь, как [color=darkred]ткань под кожей[/color] медленно умирает...
virus-necrosis-popup-2 = [color=darkred]Боль пронзает[/color] тело, кожа будто [color=crimson]гниёт изнутри[/color].
virus-necrosis-popup-3 = Твоё тело откликается на инфекцию — [color=purple]клетки разрушаются[/color] одна за другой.
virus-necrosis-popup-4 = Из-под кожи выступает [color=darkred]чёрная жидкость[/color], сопровождаемая жжением.
virus-necrosis-popup-5 = Ты чувствуешь [color=purple]тяжесть и разложение[/color] в собственных мышцах.

# Диагност вирусов
virus-diagnoser-dna-material-attached = Днк материал внутри машины.
virus-diagnoser-flask-attached = Колба внутри машины.

virus-collector-no-mouth = У цели нет ротового отверстия. Введение вируса невозможно.
virus-collector-is-used = Предмет уже был использован.
virus-collector-warn-target = Вам лезут в рот.
drug-collector-dna-not-found = Неизвестно.

reagent-name-viral-solution = вирусный раствор
reagent-desc-viral-solution = Стерильный физиологический раствор с суспензией активного квантового вируса, способного выдерживать FTL-транспортировку.
reagent-physical-desc-clear = прозрачная жидкость

reagent-effect-guidebook-cause-virus =
    { $chance ->
        [1] Заражает
       *[other] заражает
    } вирусом

## -----------------------
##   Вирусный отчёт
## -----------------------

virus-report-no-virus = Вирусных данных не найдено. Образец чист.

virus-report-title = АНАЛИЗ ВИРУСНОГО ОБРАЗЦА

virus-report-strain = Идентификатор штамма: {$id}
virus-report-threshold = Состояние вируса (живучесть): {$value}
virus-report-infectivity = Инфективность: {$value}%
virus-report-complexity = Сложность разработки вакцины: {$value}

virus-report-default-medicine-resistance = Базовое сопротивление медикаментам: {$value}

virus-report-medicine-header = Устойчивость к препаратам:
virus-report-medicine-entry = - {$name}: {$value}

virus-report-medicine-none = Не обнаружена

virus-report-symptoms-header = Активные симптомы:
virus-report-symptoms-none = Не выявлены

virus-report-bodyes-header = Допустимые к заражению организмы:
virus-report-body-any = Не выявлены

virus-report-footer = Отчёт сгенерирован вирусным диагностическим модулем.

## UI

### Заголовок окна
virus-diagnoser-window-title = Диагност заболеваний

### Вкладка сервера
virus-diagnoser-server-strains-label = Штаммы вируса на сервере
virus-diagnoser-delete-strain-button = Удалить штамм

virus-diagnoser-server-missing = Нет соединения с сервером данных
virus-diagnoser-server-far = Сервер данных находится слишком далеко

### Вкладка диагноста
virus-diagnoser-actions-label = Доступные действия

virus-diagnoser-scan-button = Сканировать вирус
virus-diagnoser-print-button = Печать отчёта
virus-diagnoser-generate-button = Сгенерировать вирус

virus-diagnoser-missing = Нет соединения с диагностом
virus-diagnoser-far = Диагност находится слишком далеко

# Solution аналайзер
virus-solution-analyser-start-analys-button = Запустить анализ
virus-solution-analyser-missing = Нет соединения с анализатором веществ
virus-solution-analyser-far = Анализатор веществ находится слишком далеко

## UI Evolution

### Заголовок окна
virus-evolution-window-title = Эволюция вируса

### Вкладка симптомов
virus-evolution-available-symptoms = Доступные симптомы
virus-evolution-active-symptoms = Активные симптомы
virus-evolution-buy-button = Купить
virus-evolution-not-enough-points = Недостаточно очков мутации

### Вкладка whitelist
virus-evolution-whitelist = Доступные организмы
virus-evolution-buy-body = Разблокировать организм

### Описание
virus-evolution-description-header = Описание симптома

### Ошибки
virus-evolution-error-no-selection = Симптом не выбран
virus-evolution-error-body-no-selection = Организм не выбран

### Стоимость
virus-evolution-cost-label = Стоимость: { $cost }
virus-evolution-mutation-points = Очки мутации: { $points }

# Ports

signal-port-name-virus-diagnoser-sender = Диагност заболеваний
signal-port-description-virus-diagnoser-sender = Передатчик сигнала диагносту заболеваний

signal-port-name-virus-data-server-sender = Сервер данных
signal-port-description-virus-data-server-sender = Передатчик сигнала серверу данных

signal-port-name-virus-solution-analyzer-sender = Диагност веществ
signal-port-description-virus-solution-analyzer-sender = Передатчик сигнала диагносту веществ

signal-port-name-virus-diagnoser-receiver = Диагност заболеваний
signal-port-description-virus-diagnoser-receiver = Принимающий сигнал диагност заболеваний

signal-port-name-virus-data-server-receiver = Сервер данных
signal-port-description-virus-data-server-receiver = Принимающий сигнал сервер данных

signal-port-name-virus-solution-analyzer-receiver = Диагност веществ
signal-port-description-virus-solution-analyzer-receiver = Принимающий сигнал диагност веществ

# Другое

research-technology-virology = Вирусология

virus-mutation-verb = Очистить от вируса