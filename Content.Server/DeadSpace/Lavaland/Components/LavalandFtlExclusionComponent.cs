namespace Content.Server.DeadSpace.Lavaland.Components;

[RegisterComponent]
public sealed partial class LavalandFtlExclusionComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Range = 32f;
}
