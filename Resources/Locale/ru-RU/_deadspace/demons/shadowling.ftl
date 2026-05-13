objective-issuer-shadowling = [color=red]Тенеморф[/color]
objective-issuer-shadowlingslave = [color=red]Слуга Тенеморфа[/color]
ghost-role-information-shadowling-name = Тенеморф
ghost-role-information-shadowling-description = Вы - тенеморф, порождение тьмы, охотящееся за разумами смертных.
ghost-role-information-shadowling-rules = Вы не помните ничего из своей предыдущей жизни, если администратор не сказал вам обратное.
    Вам разрешается помнить знания об игре в целом, например, как готовить, как использовать предметы и т.д.
    Вам [color=red]НЕ[/color] разрешается помнить, имя, внешность и т.д. вашего предыдущего персонажа.
    Вы антагонист. Ваша цель - порабощать разумы экипажа и уничтожить всех, кто не подчинится вашей воле.
shadowling-role-name = Тенеморф
roles-antag-shadowling-name = Тенеморф
roles-antag-shadowling-objective = Ваша задача - захватить станцию, подчинив себе разумы членов экипажа и уничтожив всех неугодных вашей воле.
admin-verb-text-make-shadowling = Сделать тенеморфом
shadowling-recruit-objective-title = Порабощение
shadowling-recruit-objective-desc = Подчините разумы персонала станции. Порабощено: {$current}/{$target}. При достижении цели вы сможете превознестись.

roles-antag-shadowlingslave-name = Слуга Тенеморфа
roles-antag-shadowlingslave-objective = Ваша задача - защищать и выполнять приказы Тенеморфа, а также всеми силами добиться его вознесения.

shadowling-role-greeting =
    Вы - Тенеморф. 
    Найдите укромное место и раскройтесь. Подчиняйте себе разумы членов экипажа, чтобы превознестись. Помните - свет убивает вас.

shadowling-role-briefing =
    С помощью своих способностей подчиняйте членов экипажа без импланта защиты разума.
    Будьте аккуратны: если на станции узнают о вас - на вас начнётся охота!
    По мере подчинения экипажа, вам будет открываться больше способностей.
    Поработите достаточно разумов, чтобы превознестись и уничтожить эту станцию!
shadowlingslave-role-briefing =
    Выполняйте все приказы, данные вам Тенеморфом.
role-subtype-shadowling = Тенеморф
role-subtype-shadowling-slave = Слуга Тенеморфа
shadowling-ascendance-announcement = Внимание! Мы фиксируем вознесение тенеморфа на вашей станции! Сектор полностью изолирован, любые попытки покинуть его будут пресекаться огнём Блюспейс Артиллерии. Для локализации и нейтрализации угрозы будет задействован флот NanoTrasen. Благодарим за сотрудничество и преданность корпорации!
shadowling-ascendance-sender = Департамент Вооружённых Сил NanoTrasen
shadowling-title = Тенеморфы
shadowling-description = Тенеморфы среди нас!

shadowling-round-end-count =
    { $initialCount ->
        [one] Тенеморф был один:
       *[other] Тенеморфов было { $initialCount }:
    }
shadowling-round-end-name-user = [color=#c00000]{ $name }[/color] ([color=gray]{ $username }[/color]) поработил { $count } { $count ->
        [one] члена
        [few] члена
       *[other] членов
    } экипажа

shadowling-win = Тенеморф вознёсся! Тьма поглотила станцию.
shadowling-lose = Все тенеморфы уничтожены.
shadowling-stalemate = Тенеморф не смог вознестись.
shadowling-alert-announcement = Внимание экипажу станции! На связи Секторальный Штаб ЦК! Нами зафиксирована аномальная демоническая активность в вашем секторе. Вам будет отправлен специальный Отряд Быстрого Реагирования для локализации и нейтрализации источника угрозы. Всему персоналу станции следовать указаниям Службы Безопасности. Ожидайте прибытия в кратчайшие сроки. Слава NanoTrasen!
shadowling-alert-sender = Центральное Командование