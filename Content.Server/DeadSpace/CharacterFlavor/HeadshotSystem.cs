using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Shared.DeadSpace.CharacterFlavor;
using Robust.Shared.Player;

namespace Content.Server.DeadSpace.CharacterFlavor;

public sealed class HeadshotSystem : SharedHeadshotSystem
{
    private static readonly HttpClient HttpClient = new();

    public override void Initialize()
    {
        base.Initialize();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        SubscribeNetworkEvent<RequestHeadshotDownloadEvent>(OnRequestHeadshotDownload);
        SubscribeNetworkEvent<RequestHeadshotExamineEvent>(OnRequestHeadshotExamine);
    }

    protected override async void OpenHeadshotFlavor(EntityUid actor, EntityUid target)
    {
        base.OpenHeadshotFlavor(actor, target);

        if (!TryComp<HeadshotComponent>(target, out var headshot))
            return;

        if (string.IsNullOrWhiteSpace(headshot.HeadshotData))
            return;

        if (!headshot.HeadshotData.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !headshot.HeadshotData.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return;

        var image = await DownloadImageAsync(headshot.HeadshotData);
        var ev = new HeadshotExamineResultEvent(GetNetEntity(target), image, headshot.FlavorText);
        RaiseNetworkEvent(ev, actor);
    }

    private async void OnRequestHeadshotDownload(RequestHeadshotDownloadEvent ev, EntitySessionEventArgs args)
    {
        if (!IsValidHeadshotUrl(ev.Url))
        {
            RaiseNetworkEvent(new HeadshotDownloadResultEvent(null, false), Filter.SinglePlayer(args.SenderSession));
            return;
        }

        var imageBytes = await DownloadImageAsync(ev.Url);
        if (imageBytes == null)
        {
            RaiseNetworkEvent(new HeadshotDownloadResultEvent(null, false), Filter.SinglePlayer(args.SenderSession));
            return;
        }

        var base64 = Convert.ToBase64String(imageBytes);
        RaiseNetworkEvent(new HeadshotDownloadResultEvent(base64, true), Filter.SinglePlayer(args.SenderSession));
    }

    private async void OnRequestHeadshotExamine(RequestHeadshotExamineEvent ev, EntitySessionEventArgs args)
    {
        var target = GetEntity(ev.Target);
        if (!TryComp<HeadshotComponent>(target, out var headshot))
            return;

        if (string.IsNullOrWhiteSpace(headshot.HeadshotData))
            return;

        if (!headshot.HeadshotData.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !headshot.HeadshotData.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return;

        var image = await DownloadImageAsync(headshot.HeadshotData);
        var result = new HeadshotExamineResultEvent(ev.Target, image, headshot.FlavorText);
        RaiseNetworkEvent(result, Filter.SinglePlayer(args.SenderSession));
    }

    private static async Task<byte[]?> DownloadImageAsync(string url)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return null;

            const int maxSize = 5 * 1024 * 1024;
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var ms = new MemoryStream();
            var buffer = new byte[8192];
            int totalRead = 0;
            while (true)
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                    break;
                totalRead += read;
                if (totalRead > maxSize)
                    return null;
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download image from {url}: {ex}");
            return null;
        }
    }

    private static bool IsValidHeadshotUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        if (url.Length > 1000)
            return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        return true;
    }
}
