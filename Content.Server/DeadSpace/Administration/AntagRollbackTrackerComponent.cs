// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Server.Antag;

namespace Content.Server.DeadSpace.Administration;

/// <summary>
/// Records entity changes made while assigning antagonist definitions so an administrator can
/// return the character to their pre-antagonist state without stripping unrelated round changes.
/// </summary>
[RegisterComponent, Access(typeof(AntagSelectionSystem))]
public sealed partial class AntagRollbackTrackerComponent : Component
{
    public readonly Dictionary<EntityUid, HashSet<string>> AddedComponents = [];
    public readonly HashSet<EntityUid> GrantedEntities = [];
    public readonly HashSet<EntityUid> ObjectivesBeforeAssignment = [];
    public bool ObjectiveSnapshotTaken;
}
