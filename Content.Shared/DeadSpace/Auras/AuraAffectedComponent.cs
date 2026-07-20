// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Auras;

/// <summary>
/// Added dynamically by AuraSystem when an entity overlaps with one or more auras.
/// Handled fully in prediction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(AuraSystem))]
public sealed partial class AuraAffectedComponent : Component
{
    /// <summary>
    /// Auras currently affecting this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> ActiveAuras = new();

    /// <summary>
    /// The visual entity spawned to represent the aura effect (handled purely locally to prevent prediction dupes).
    /// </summary>
    [DataField]
    public EntityUid? VisualEntity;
}
