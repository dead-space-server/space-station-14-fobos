frostheim-role-name = Выживший Фростхейма
frostheim-role-description = Вы — один из выживших после крушения на ледяной планете. Выживайте, сотрудничайте, найдите путь к спасению.
frostheim-role-rules = Работайте в команде. Ваша цель — выжить.

frostheim-examine-captain-self = Я — капитан. Эти люди рассчитывают на меня.
frostheim-examine-captain-1 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] был нашим капитаном. Я всегда знал его как надёжного лидера.
        [female] была нашим капитаном. Я всегда знал её как надёжного лидера.
       *[other] было нашим капитаном.
    }
frostheim-examine-captain-2 = Я { GENDER($target) ->
        [male] помню его
        [female] помню её
       *[other] помню
    } — { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] вёл нас через самые тяжёлые времена.
        [female] вела нас через самые тяжёлые времена.
       *[other] вело нас через самые тяжёлые времена.
    }
frostheim-examine-captain-3 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] — тот, кому доверяли все на борту. Наш капитан.
        [female] — та, кому доверяли все на борту. Наш капитан.
       *[other] — тот, кому доверяли все на борту. Наш капитан.
    }

frostheim-examine-engineer-self = Я — инженер. Если что-то сломается, я это починю.
frostheim-examine-engineer-1 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] был нашим инженером. Без него мы бы замёрзли в первую же ночь.
        [female] была нашим инженером. Без неё мы бы замёрзли в первую же ночь.
       *[other] было нашим инженером.
    }
frostheim-examine-engineer-2 = Я { GENDER($target) ->
        [male] помню его руки
        [female] помню её руки
       *[other] помню
    } — вечно в мазуте, но { GENDER($target) ->
        [male] он мог починить что угодно.
        [female] она могла починить что угодно.
       *[other] могло починить что угодно.
    }
frostheim-examine-engineer-3 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] знал каждый болт на этой станции. Наш инженер.
        [female] знала каждый болт на этой станции. Наш инженер.
       *[other] знало каждый болт на этой станции. Наш инженер.
    }

frostheim-examine-security-self = Я — охранник. Моя задача — защитить остальных.
frostheim-examine-security-1 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] стоял на страже, пока мы спали. Наш охранник.
        [female] стояла на страже, пока мы спали. Наш охранник.
       *[other] стояло на страже, пока мы спали. Наш охранник.
    }
frostheim-examine-security-2 = Я { GENDER($target) ->
        [male] чувствовал себя в безопасности рядом с ним.
        [female] чувствовал себя в безопасности рядом с ней.
       *[other] чувствовал себя в безопасности.
    } { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] никогда не подводил.
        [female] никогда не подводила.
       *[other] никогда не подводило.
    }
frostheim-examine-security-3 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] был готов защитить каждого из нас ценой своей жизни.
        [female] была готова защитить каждого из нас ценой своей жизни.
       *[other] было готово защитить каждого из нас.
    }

frostheim-examine-medic-self = Я — медик. Я должен сохранить жизни этих людей.
frostheim-examine-medic-1 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] был нашим медиком. Его руки спасли не одну жизнь.
        [female] была нашим медиком. Её руки спасли не одну жизнь.
       *[other] было нашим медиком.
    }
frostheim-examine-medic-2 = Я { GENDER($target) ->
        [male] обязан ему жизнью.
        [female] обязан ей жизнью.
       *[other] обязан жизнью.
    } { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] вытащил меня с того света.
        [female] вытащила меня с того света.
       *[other] вытащило меня с того света.
    }
frostheim-examine-medic-3 = { CAPITALIZE(SUBJECT($target)) } { GENDER($target) ->
        [male] всегда знал, что делать, когда кто-то ранен. Наш доктор.
        [female] всегда знала, что делать, когда кто-то ранен. Наш доктор.
       *[other] всегда знало, что делать, когда кто-то ранен. Наш доктор.
    }
