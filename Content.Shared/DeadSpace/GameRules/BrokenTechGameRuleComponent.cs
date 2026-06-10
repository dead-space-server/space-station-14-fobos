// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT 

using System.Collections.Generic;
using System.Numerics;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeadSpace.GameRules.Components;

[RegisterComponent]
public sealed partial class BrokenTechPowerDisabledComponent : Component
{
    
}

[RegisterComponent]
public sealed partial class BrokenTechGameRuleComponent : Component
{
    [DataField]
    public List<BrokenTechEntry> ListComponent = new();
}

[DataDefinition]
public sealed partial class BrokenTechEntry
{
    [DataField]
    public string ComponentName = string.Empty;

    [DataField]
    public float MinuteMin = 20f;

    [DataField]
    public float MinuteMax = 30f;

    [DataField]
    public int HowMuchEntity = 1;

    [DataField]
    public int Chance = 100;

    [DataField]
    public BrokenTechAction Action = new BlockWorkingEntityAction();
    
    [DataField]
    public List<EntProtoId> BlacklistPrototypes = new();
    
    [DataField]
    public List<ProtoId<TagPrototype>> BlacklistTags = new();

    public float ElapsedSeconds;
    public float NextAttemptSeconds = -1f;
    public bool Triggered;
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class BrokenTechAction { }

[DataDefinition]
public sealed partial class ExplodeEntityAction : BrokenTechAction
{
    [DataField]
    public EntityTableSelector? SpawnTable;

    [DataField]
    public Vector2 SpawnOffset = new(1f, 0f);

    [DataField]
    public bool UseEntityRotation = false;

    [DataField]
    public float ExplosionIntensity = 3f;

    [DataField]
    public string ExplosionType = "Default";
}

[DataDefinition]
public sealed partial class BlockWorkingEntityAction : BrokenTechAction
{
    [DataField]
    public EntityTableSelector? SpawnTable;

    [DataField]
    public Vector2 SpawnOffset = Vector2.Zero;

    [DataField]
    public bool UseEntityRotation = false;
}