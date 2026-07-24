// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Objectives.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Server.Objectives.Systems;
using Content.Server.Objectives.Components;
using System;

namespace Content.Server.DeadSpace.Hooligan.Objectives;


/// <summary>
/// Логика цели "Приподать урок": считает урон, нанесённый Хулиганом жертве,
/// вешает компонент-счётчик на жертву и отвечает на вопрос о прогрессе.
/// </summary>
public sealed class HooliganBeatConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HooliganBeatConditionComponent, ObjectiveAfterAssignEvent>(OnAssign);
        SubscribeLocalEvent<HooliganBeatTargetComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<HooliganBeatConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    // Вешает компонент-счётчик на жертву.
    private void OnAssign(Entity<HooliganBeatConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (args.Mind.OwnedEntity is not {} attacker)
            return;

        if (!_target.GetTarget(ent.Owner, out var victimMind))
            return;
        if (!TryComp<MindComponent>(victimMind, out var victimMindComp))
            return;
        if (victimMindComp.OwnedEntity is not {} victim)
            return;

        var tracker = EnsureComp<HooliganBeatTargetComponent>(victim);
        tracker.Objective = ent.Owner;
        tracker.Attacker = attacker;
    }

    // По телу с компонентном-счётчиком прошёл урон.
    // Если бил Хулиган с правильным айди, прибавить к счётчику.
    private void OnDamaged(Entity<HooliganBeatTargetComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;
        if (args.Origin != ent.Comp.Attacker)
            return;
        if (!TryComp<HooliganBeatConditionComponent>(ent.Comp.Objective, out var condition))
            return;
        condition.DamageDealt += args.DamageDelta.GetTotal();
    }

    private void OnGetProgress(Entity<HooliganBeatConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (!TryComp<NumberObjectiveComponent>(ent.Owner, out var number) || number.Target == 0)
        {
            args.Progress = 1f;
            return;
        }
        args.Progress = MathF.Min(ent.Comp.DamageDealt.Float() / number.Target, 1f);
    }
}