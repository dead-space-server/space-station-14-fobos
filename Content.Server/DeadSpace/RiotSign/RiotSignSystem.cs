using Content.Shared.DeadSpace.RiotSign;
using Robust.Server.GameObjects;
using Content.Shared.Interaction.Events;

namespace Content.Server.DeadSpace.RiotSign;

public sealed class RiotSignSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LabelableComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<LabelableComponent, LabelChangedMessage>(OnLabelChanged);
    }

    private void OnUseInHand(EntityUid uid, LabelableComponent component, UseInHandEvent args)
    {
        _uiSystem.TryOpenUi(uid, LabelUiKey.Key, args.User);
    }

    private void OnLabelChanged(EntityUid uid, LabelableComponent component, LabelChangedMessage args)
    {
        var meta = MetaData(uid);

        if (string.IsNullOrEmpty(component.OriginalName))
            component.OriginalName = meta.EntityName;

        var newText = args.Text.Trim();
        string finalName;

        if (newText.Length > 100)
        {
            newText = newText[..100];
        }

        if (string.IsNullOrWhiteSpace(newText))
        {
            finalName = component.OriginalName;
        }
        else
        {
            finalName = $"{component.OriginalName} ({newText})";
        }

        _metaData.SetEntityName(uid, finalName, meta);
    }
}
