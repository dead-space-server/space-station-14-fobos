using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Client.DeadSpace.BSAConsole;

/// <summary>
/// Extended NavMapControl for BSA console that raises an event on map click with world coordinates.
/// </summary>
public sealed partial class BSANavMapControl : NavMapControl
{
    /// <summary>
    /// Raised when user left-clicks on the map. Provides map-space coordinates.
    /// </summary>
    public Action<MapCoordinates>? OnMapClick;

    public BSANavMapControl()
    {
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        if (args.Function == EngineKeyFunctions.UIClick && OnMapClick != null)
        {
            var mapUid = MapUid;
            if (mapUid != null && EntManager.TryGetComponent<TransformComponent>(mapUid.Value, out var xform)
                && EntManager.TryGetComponent<PhysicsComponent>(mapUid.Value, out var physics))
            {
                var offset = Offset + physics.LocalCenter;
                var localPosition = args.PointerLocation.Position - GlobalPixelPosition;
                var unscaledPosition = (localPosition - MidPointVector) / MinimapScale;
                var worldPosition = Vector2.Transform(
                    new Vector2(unscaledPosition.X, -unscaledPosition.Y) + offset,
                    EntManager.System<SharedTransformSystem>().GetWorldMatrix(xform));
                var mapPos = new MapCoordinates(worldPosition, xform.MapID);
                OnMapClick.Invoke(mapPos);
                args.Handle();
                return;
            }
        }

        base.KeyBindUp(args);
    }
}
