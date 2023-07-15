using System.Net;

namespace ImageScraping;

public static class ImageDownloader
{
    public static void DownloadImage(string url, string savePath, int index)
    {
        using var client = new WebClient();
        var path = Path.Combine(savePath, $"{index}.png");
        client.DownloadFile(new Uri(url), path);
    }
}