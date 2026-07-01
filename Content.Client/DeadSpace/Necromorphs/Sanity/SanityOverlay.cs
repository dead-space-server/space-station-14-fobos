using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
namespace Content.Client.DeadSpace.Sanity;

public sealed class SanityOverlay : Overlay
{
    private readonly float _outerCircleValue = 80f;
    private readonly float _innerCircleValue = 40f;
    private readonly float _outerCircleMaxRadius = 60f;
    private readonly float _innerCircleMaxRadius = 60f;
    public float Value = 0f;
    private readonly float _darknessAlphaOuter = 0.94f;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _circleMaskShader = default!;
    private const string ShaderName = "DeadSpaceGradientCircleMask";

    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public SanityOverlay()
    {
        IoCManager.InjectDependencies(this);
        _circleMaskShader = _prototypeManager.Index<ShaderPrototype>(ShaderName).InstanceUnique();
    }
    protected override void Draw(in OverlayDrawArgs args)
    {
        _circleMaskShader.SetParameter("color", Vector3.Zero);
        _circleMaskShader.SetParameter("outerCircleRadius", _outerCircleValue);
        _circleMaskShader.SetParameter("innerCircleRadius", _innerCircleValue);
        _circleMaskShader.SetParameter("outerCircleMaxRadius", _outerCircleMaxRadius);
        _circleMaskShader.SetParameter("innerCircleMaxRadius", _innerCircleMaxRadius);
        _circleMaskShader.SetParameter("time", Value);
        _circleMaskShader.SetParameter("darknessAlphaOuter", _darknessAlphaOuter);

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;

        worldHandle.UseShader(_circleMaskShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
