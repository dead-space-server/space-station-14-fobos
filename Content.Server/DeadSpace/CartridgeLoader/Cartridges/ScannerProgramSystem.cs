using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.DeadSpace.CartridgeLoader.Cartridges;
using Content.Shared.Paper;

namespace Content.Server.DeadSpace.CartridgeLoader.Cartridges;

public sealed class ScannerProgramSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScannerProgramComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<ScannerProgramComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<ScannerProgramComponent, CartridgeAfterInteractEvent>(OnAfterInteract);
    }

    private void OnUiReady(EntityUid uid, ScannerProgramComponent component, CartridgeUiReadyEvent args)
    {
        UpdateUiState(uid, args.Loader, component);
    }

    private void OnUiMessage(EntityUid uid, ScannerProgramComponent component, CartridgeMessageEvent args)
    {
        if (args is not ScannerProgramUiMessageEvent message)
            return;

        switch (message.Action)
        {
            case ScannerProgramUiAction.Select:
                break;
            case ScannerProgramUiAction.Delete:
                if (message.DocumentIndex >= 0 && message.DocumentIndex < component.Documents.Count)
                    component.Documents.RemoveAt(message.DocumentIndex);
                break;
            case ScannerProgramUiAction.Clear:
                component.Documents.Clear();
                break;
            case ScannerProgramUiAction.Rename:
                if (message.DocumentIndex >= 0 && message.DocumentIndex < component.Documents.Count && !string.IsNullOrWhiteSpace(message.NewName))
                {
                    var doc = component.Documents[message.DocumentIndex];
                    var newList = new List<StampDisplayInfo>(doc.StampedBy);
                    component.Documents[message.DocumentIndex] = new ScannedDocument(message.NewName, doc.Content, newList);
                }
                break;
        }

        UpdateUiState(uid, GetEntity(args.LoaderUid), component);
    }

    private void OnAfterInteract(EntityUid uid, ScannerProgramComponent component, CartridgeAfterInteractEvent args)
    {
        var interactEvent = args.InteractEvent;

        if (!interactEvent.CanReach || interactEvent.Target == null)
            return;

        var target = interactEvent.Target.Value;

        if (!TryComp<PaperComponent>(target, out var paper))
            return;

        if (!TryComp(target, out MetaDataComponent? metadata))
            return;

        var doc = new ScannedDocument(
            metadata.EntityName,
            paper.Content,
            new List<StampDisplayInfo>(paper.StampedBy)
        );

        component.Documents.Add(doc);

        UpdateUiState(uid, args.Loader, component);
    }

    private void UpdateUiState(EntityUid uid, EntityUid loaderUid, ScannerProgramComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = new ScannerProgramUiState(component.Documents, -1);
        _cartridgeLoaderSystem?.UpdateCartridgeUiState(loaderUid, state);
    }
}
