using System.Linq;
using Content.Shared.Construction.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.Placement;
using Robust.Client.ResourceManagement;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
// DS14-start
using Content.Client.DeadSpace.AdminToy;
using Content.Shared.DeadSpace.AdminToy;
using Robust.Client.Player;
// DS14-end

namespace Content.Client.Construction
{
    public sealed class ConstructionPlacementHijack : PlacementHijack
    {
        private readonly ConstructionSystem _constructionSystem;
        private readonly ConstructionPrototype? _prototype;

        public ConstructionSystem? CurrentConstructionSystem { get { return _constructionSystem; } }
        public ConstructionPrototype? CurrentPrototype { get { return _prototype; } }

        public override bool CanRotate { get; }

        public ConstructionPlacementHijack(ConstructionSystem constructionSystem, ConstructionPrototype? prototype)
        {
            _constructionSystem = constructionSystem;
            _prototype = prototype;
            CanRotate = prototype?.CanRotate ?? true;
        }

        /// <inheritdoc />
        public override bool HijackPlacementRequest(EntityCoordinates coordinates)
        {
            if (_prototype != null)
            {
                var dir = Manager.Direction;
                // DS14-start
                if (TryGetAdminToySystem(out var adminToy))
                    adminToy.PlaceConstructionGhost(_prototype, coordinates, dir.ToAngle());
                else
                // DS14-end
                    _constructionSystem.SpawnGhost(_prototype, coordinates, dir);
            }
            return true;
        }

        /// <inheritdoc />
        public override bool HijackDeletion(EntityUid entity)
        {
            // DS14-start
            var entityManager = IoCManager.Resolve<IEntityManager>();

            if (entityManager.TryGetComponent<ConstructionGhostComponent>(entity, out var ghost))
            {
                if (TryGetAdminToySystem(out var adminToy))
                    adminToy.ClearConstructionGhost(ghost.GhostId);
                else
                // DS14-end
                    _constructionSystem.ClearGhost(entity.GetHashCode());
            }
            return true;
        }

        /// <inheritdoc />
        public override void StartHijack(PlacementManager manager)
        {
            base.StartHijack(manager);

            if (_prototype is null || !_constructionSystem.TryGetRecipePrototype(_prototype.ID, out var targetProtoId))
                return;

            if (!IoCManager.Resolve<IPrototypeManager>().TryIndex(targetProtoId, out EntityPrototype? proto))
                return;

            manager.CurrentTextures = SpriteComponent.GetPrototypeTextures(proto, IoCManager.Resolve<IResourceCache>()).ToList();
        }

        // DS14-start
        private static bool TryGetAdminToySystem(out AdminToySystem adminToy)
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            adminToy = entityManager.System<AdminToySystem>();

            return IoCManager.Resolve<IPlayerManager>().LocalEntity is { } localEntity &&
                   entityManager.HasComponent<AdminToyComponent>(localEntity);
        }
        // DS14-end
    }
}
