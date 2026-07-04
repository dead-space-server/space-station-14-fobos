using System.IO;
using Content.Shared.DeadSpace.CharacterFlavor;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client.DeadSpace.CharacterFlavor;

public sealed class HeadshotUIController : UIController
{
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private HeadshotExamineWindow? _examineWindow;
    private Action<string?>? _pendingDownloadCallback;

    public void OpenExamineWindow(EntityUid target)
    {
        _examineWindow?.Close();
        _examineWindow = new HeadshotExamineWindow(this);

        if (!EntityManager.TryGetComponent<HeadshotComponent>(target, out var headshot))
        {
            _examineWindow.Close();
            _examineWindow = null;
            return;
        }

        _examineWindow.SetFlavorText(headshot.FlavorText);
        _examineWindow.OpenCentered();

        if (string.IsNullOrWhiteSpace(headshot.HeadshotData))
            return;

        if (headshot.HeadshotData.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
        {
            var base64Data = headshot.HeadshotData;
            var commaIndex = base64Data.IndexOf(',');
            if (commaIndex >= 0 && commaIndex < base64Data.Length - 1)
            {
                base64Data = base64Data[(commaIndex + 1)..];
                try
                {
                    var bytes = Convert.FromBase64String(base64Data);
                    var texture = LoadTextureFromBytes(bytes);
                    if (texture != null)
                        _examineWindow.SetHeadshotTexture(texture);
                }
                catch
                {
                }
            }
        }
        else if (headshot.HeadshotData.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 headshot.HeadshotData.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            _examineWindow.ShowLoading();
            var netTarget = EntityManager.GetNetEntity(target);
            _net.SendSystemNetworkMessage(new RequestHeadshotExamineEvent(netTarget));
        }
    }

    public void OnHeadshotDownloadResult(HeadshotDownloadResultEvent ev)
    {
        if (_pendingDownloadCallback != null)
        {
            var callback = _pendingDownloadCallback;
            _pendingDownloadCallback = null;
            callback(ev.Success ? ev.Base64 : null);
        }
    }

    public void OnHeadshotExamineResult(HeadshotExamineResultEvent ev)
    {
        if (_examineWindow is not { Disposed: false })
            return;

        if (ev.Image is { Length: > 0 })
        {
            var texture = LoadTextureFromBytes(ev.Image);
            if (texture != null)
                _examineWindow.SetHeadshotTexture(texture);
        }
        else
        {
            _examineWindow.HideLoading();
        }
    }

    public OwnedTexture? LoadTextureFromBytes(byte[] imageBytes)
    {
        try
        {
            var stream = new MemoryStream(imageBytes);
            return _clyde.LoadTextureFromPNGStream(stream, "headshot");
        }
        catch
        {
            return null;
        }
    }

    public OwnedTexture? LoadTextureFromBase64(string base64)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return LoadTextureFromBytes(bytes);
        }
        catch
        {
            return null;
        }
    }

    public void RequestUrlDownload(string url, Action<string?> onComplete)
    {
        _pendingDownloadCallback = onComplete;
        _net.SendSystemNetworkMessage(new RequestHeadshotDownloadEvent(url));
    }

    public void CloseExamineWindow()
    {
        _examineWindow?.Close();
        _examineWindow = null;
    }
}
