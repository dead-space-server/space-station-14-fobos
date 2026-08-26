using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Client.DeadSpace.BSAConsole;

public sealed partial class BSANavMapControl : NavMapControl
{
    public Action<MapCoordinates>? OnMapClick;

    public BSANavMapControl()
    {
    }

    public Vector2? WorldToScreen(MapCoordinates coords)
    {
        if (MapUid == null)
            return null;

        if (!EntManager.TryGetComponent<TransformComponent>(MapUid.Value, out var xform))
            return null;

        if (!EntManager.TryGetComponent<PhysicsComponent>(MapUid.Value, out var physics))
            return null;

        var transformSystem = EntManager.System<SharedTransformSystem>();
        var offset = Offset + physics.LocalCenter;
        var localPos = Vector2.Transform(coords.Position, transformSystem.GetInvWorldMatrix(xform)) - offset;
        return ScalePosition(new Vector2(localPos.X, -localPos.Y));
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        if (args.Function == EngineKeyFunctions.UIClick && OnMapClick != null)
        {
            var mapUid = MapUid;
            if (mapUid != null
                && EntManager.TryGetComponent<TransformComponent>(mapUid.Value, out var xform)
                && EntManager.TryGetComponent<PhysicsComponent>(mapUid.Value, out var physics))
            {
                var transformSystem = EntManager.System<SharedTransformSystem>();
                var offset = Offset + physics.LocalCenter;
                var localPosition = args.PointerLocation.Position - GlobalPixelPosition;
                var unscaledPosition = (localPosition - MidPointVector) / MinimapScale;
                var worldPosition = Vector2.Transform(
                    new Vector2(unscaledPosition.X, -unscaledPosition.Y) + offset,
                    transformSystem.GetWorldMatrix(xform));
                var mapPos = new MapCoordinates(worldPosition, xform.MapID);
                OnMapClick.Invoke(mapPos);
                args.Handle();
                return;
            }
        }

        base.KeyBindUp(args);
    }
}
