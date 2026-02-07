virus-resistance-coefficient-value = - Шанс [color=violet]заражения вирусом[/color] снижен на [color=purple]{ $value }%[/color].

virus-data-server-get-disk-verb-text = Вытащить диск с данными { $value }.

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

reagent-effect-guidebook-damage-disease =
    { $chance ->
        [1] Убивает
       *[other] убивает
    } болезни или вирусы в организме

## -----------------------
##   Вирусный отчёт
## -----------------------

virus-report-no-virus = Вирусных данных не найдено. Образец чист.

virus-report-title = АНАЛИЗ ВИРУСНОГО ОБРАЗЦА

virus-report-strain = Идентификатор штамма: {$id}
virus-report-threshold = Состояние вируса (живучесть): {$value}
virus-report-infectivity = Инфективность: {$value}%

virus-report-damage-when-dead = Показатель уязвимости, если организм носителя мёртв: {$value}
virus-report-mutation-points = Очки мутации: {$value}
virus-report-regen-threshold = Регенерация вируса: {$value}
virus-report-regen-mutation = Скорость мутации: {$value}
virus-report-milty-price-delete-symptom = Сложность удаления симптома {$value}

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


# Консоль эволюции

### WINDOW ###

virus-evolution-window-title = Эволюция вируса


### TABS ###

virus-evolution-tab-evolution = Эволюция
virus-evolution-tab-whitelist = Белый список


### EVOLUTION TAB ###

virus-evolution-available-symptoms = Доступные симптомы
virus-evolution-active-symptoms = Активные симптомы
virus-evolution-description-header = Описание
virus-evolution-buy-button = Купить симптом
virus-evolution-delete-button = Удалить симптом

virus-evolution-mutation-points =
    Очки мутации: { $points }

virus-evolution-health =
    Максимум здоровья: { $max }

virus-evolution-infectivity =
    Заразность: { $percent }%

virus-evolution-infected-count =
    Заражённых: { $count }

virus-evolution-points-per-second =
    Очков/сек: { $points }

virus-evolution-cost-label =
    Стоимость: { $cost }


### WHITELIST TAB ###

virus-evolution-available-bodies = Доступные тела
virus-evolution-active-bodies = Активные тела
virus-evolution-buy-body = Добавить тело
virus-evolution-delete-body = Удалить тело

### DATASERVER STATES ###

virus-evolution-virusdata-missing =
    Данные об вирусе не найдены

virus-evolution-dataserver-missing =
    Сервер данных или анализатор веществ не подключён

virus-evolution-dataserver-far =
    Сервер данных или анализатор веществ слишком далеко


### SOLUTION ANALYZER STATES (на будущее) ###

virus-evolution-solution-analyzer-missing =
    Анализатор растворов не подключён

virus-evolution-solution-analyzer-far =
    Анализатор растворов слишком далеко


### BUTTON / ACTION ERRORS (если понадобятся) ###

virus-evolution-not-enough-points =
    Недостаточно очков мутации

virus-evolution-no-selection =
    Ничего не выбрано


### TOOLTIP / INFO ###

virus-evolution-symptom-price-tooltip =
    Базовая цена: { $price }

virus-evolution-body-price-tooltip =
    Стоимость тела: { $price }


### DEBUG / FALLBACK ###

virus-evolution-unknown-symptom =
    Неизвестный симптом

virus-evolution-unknown-body =
    Неизвестное тело


# РАЗУМНЫЙ ВИРУС

sentient-virus-infect-impossible-target = цель невозможно заразить
sentient-virus-teleport-no-primary-infected = нулевых пациентов не найдено
sentient-virus-infect-failed-source = вы больше не можете создать нулевого пациента
sentient-virus-infect-no-points = Не хватает { $price } очков мутации.
sentient-virus-infect-compensation = Ваш первичный пациент ушёл в крио, вам компенсировали { $price } очков мутации.

# ПРЕПАРАТЫ

reagent-name-infectizine = инфектизин
reagent-desc-infectizine = Простейший препарат, эффективный против слабых вирусов.

reagent-name-mycocline = микоклин
reagent-desc-mycocline = Препарат широкого спектра действия.

reagent-name-virucidine = вируцид
reagent-desc-virucidine = Агрессивный препарат, подавляющий вирусные структуры.

reagent-name-panacemycin = панацемицин
reagent-desc-panacemycin = Экспериментальный препарат экстремального действия.

ent-ChemistryBottleInfectizine = { ent-BaseChemistryBottleFilled }
    .suffix = бактеризин
    .desc = { ent-BaseChemistryBottleFilled.desc }

ent-ChemistryBottleMycocline = { ent-BaseChemistryBottleFilled }
    .suffix = микоклин
    .desc = { ent-BaseChemistryBottleFilled.desc }

ent-ChemistryBottleVirucidine = { ent-BaseChemistryBottleFilled }
    .suffix = вируцид
    .desc = { ent-BaseChemistryBottleFilled.desc }

ent-ChemistryBottlePanacemycin = { ent-BaseChemistryBottleFilled }
    .suffix = панацемицин
    .desc = { ent-BaseChemistryBottleFilled.desc }

reagent-name-septomycin = септомицин
reagent-desc-septomycin = Сильный антисептический препарат, подавляющий устойчивые штаммы инфекций.

ent-ChemistryBottleSeptomycin = { ent-BaseChemistryBottleFilled }
    .suffix = септомицин
    .desc = { ent-BaseChemistryBottleFilled.desc }

reagent-name-necrovir = некровир
reagent-desc-necrovir = Крайне токсичный противовирусный препарат, разрушающий инфекцию вместе с тканями носителя.

ent-ChemistryBottleNecrovir = { ent-BaseChemistryBottleFilled }
    .suffix = некровир
    .desc = { ent-BaseChemistryBottleFilled.desc }


# Virus-infected human accent
accent-words-virus-1 = Хрр… хрип…
accent-words-virus-2 = б-б-б… чт?
accent-words-virus-3 = Ггг… уенке
accent-words-virus-4 = ххх… пффф…
accent-words-virus-5 = бульк… ффф…
accent-words-virus-6 = хрип… хрип…
accent-words-virus-7 = м-м-м… эээ…

# ANTAG
roles-antag-sentient-virus-name = Разумный вирус
roles-antag-sentient-virus-objective = Заразите как можно больше организмов на станции.
role-subtype-sentient-virus = Разумный вирус

ghost-role-information-sentient-virus-name = Разумный вирус
ghost-role-information-sentient-virus-description = Заразите как можно больше организмов на станции.
ghost-role-information-sentient-virus-rules = Вы [color={ role-type-team-antagonist-color }][bold]{ role-type-solo-antagonist-name }[/bold][/color], распространите вирус по станции.
sentient-virus-role-greeting =
    Вы — разумный вирус.
    У вас нет тела, но есть цель.

    Проникайте в живые организмы, приспосабливайтесь к условиям станции
    и распространяйте себя любыми доступными способами.

    Используйте мутации, симптомы и носителей, чтобы выжить и усилиться.
    Чем больше заражённых, тем сильнее вы становитесь.

    Не действуйте открыто без необходимости.
    Вы, эпидемия, а не солдат.

sentient-virus-round-end-agent-name = разумный вирус

sentient-virus-title = Разумный вирус
sentient-virus-description = На станции появился разумный вирус. Он стремится заразить как можно больше организмов, мутировать и распространиться по всей станции. Будьте бдительны и не

# DataCollector
virus-collector-has-data = Образец взят у пациента.
virus-collector-not-has-data = Биологический материал не обнаружен.


health-analyzer-window-entity-infected-text =
    Заражён вирусом.
    Состояние излечения организма: { $progress }%
