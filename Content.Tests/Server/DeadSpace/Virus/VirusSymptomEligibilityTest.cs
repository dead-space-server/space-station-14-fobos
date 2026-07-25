// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Collections.Generic;
using Content.Server.DeadSpace.Virus.Systems;
using Content.Shared.DeadSpace.Virus.Prototypes;
using NUnit.Framework;
using Robust.Shared.Prototypes;

namespace Content.Tests.Server.DeadSpace.Virus;

// These pure predicate tests intentionally use isolated prototype values.
#pragma warning disable RA0039

[TestFixture]
[TestOf(typeof(VirusSystem))]
public sealed class VirusSymptomEligibilityTest
{
    private static readonly ProtoId<VirusSymptomPrototype> Candidate = "Candidate";
    private static readonly ProtoId<VirusSymptomPrototype> Required = "Required";
    private static readonly ProtoId<VirusSymptomPrototype> Blocker = "Blocker";

    [Test]
    public void AcceptsEligibleSymptom()
    {
        var active = new List<ProtoId<VirusSymptomPrototype>>();
        var symptom = new VirusSymptomPrototype();

        Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.True);
    }

    [Test]
    public void RejectsDuplicateSymptom()
    {
        var active = new List<ProtoId<VirusSymptomPrototype>> { Candidate };
        var symptom = new VirusSymptomPrototype();

        Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.False);
    }

    [Test]
    public void RestrictsTaipanOnlySymptom()
    {
        var active = new List<ProtoId<VirusSymptomPrototype>>();
        var symptom = new VirusSymptomPrototype
        {
            TaipanOnly = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.False);
            Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: true), Is.True);
        });
    }

    [Test]
    public void RequiresPrerequisiteSymptom()
    {
        var active = new List<ProtoId<VirusSymptomPrototype>>();
        var symptom = new VirusSymptomPrototype
        {
            RequiredSymptom = Required
        };

        Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.False);

        active.Add(Required);

        Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.True);
    }

    [Test]
    public void RejectsBlockedSymptom()
    {
        var active = new List<ProtoId<VirusSymptomPrototype>> { Blocker };
        var symptom = new VirusSymptomPrototype
        {
            BlockedBySymptoms = new List<ProtoId<VirusSymptomPrototype>> { Blocker }
        };

        Assert.That(VirusSystem.CanAddSymptom(active, Candidate, symptom, isTaipan: false), Is.False);
    }
}

#pragma warning restore RA0039
