using Suity.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Suity.Editor.Views.Startup;

internal static class StartupHelper
{
    public class DownloadFileInfo
    {
        public string FileName;
        public int Index;
        public int Total;
    }

    public static readonly TimeSpan CacheDuration = TimeSpan.FromDays(10);

    public static async Task DownloadExtensions(string fileInfos, Action<DownloadFileInfo> callBack = null)
    {
    }

    public static async Task DownloadFileAsync(string url, string filePath)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        await contentStream.CopyToAsync(fileStream);
    }

    public static async Task<Bitmap> DownloadLandingImage(string rFileName)
    {
        return null;
    }

    public static DateTime? GetLandingImageSaveTime(string rFileName)
    {
        return null;
    }

    public static async Task<Bitmap> DownloadCachedImage(string rFileName, string downloadBaseUrl, string imgDir, int? dpi = null)
    {
        return null;
    }

    public static async Task<Bitmap> DownloadImageAsync(string imageUrl, string filePath = null, int? dpi = null)
    {
        return null;
    }

    public static void AdjustDpiToMatchSize(Bitmap bitmap, int dpi)
    {
        if (bitmap is null)
        {
            return;
        }

        //float dpiX = bitmap.Width / (bitmap.HorizontalResolution / 25.4f);
        //float dpiY = bitmap.Height / (bitmap.VerticalResolution / 25.4f);

        bitmap.SetResolution(dpi, dpi);
    }
}