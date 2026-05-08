// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System;
using System.Collections.Generic;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeadSpace.GameRules.Components;

[RegisterComponent]
public sealed partial class BrokenTechFireSpreadComponent : Component
{
    [DataField]
    public int MaxRadius = 15;

    [DataField]
    public float SpreadDelay = 5f;

    [DataField]
    public List<ProtoId<ReagentPrototype>> WaterReagents = new() { "Water", "Holywater", "CoconutWater" };

    public EntityUid? OriginGrid;
    public Vector2i OriginTile;
    public bool HasOrigin;
    public int Distance;
    public TimeSpan NextSpread;
    public bool Finished;
}
