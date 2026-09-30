using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeachFlex.Services
{
    public sealed class CommonsImageResult
    {
        public string Title { get; set; } = "";
        public string PageUrl { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string License { get; set; } = "";
        public string LicenseUrl { get; set; } = "";
        public string Author { get; set; } = "";
        public string DisplayName => $"{Title.Replace("File:", "")} · {License}";
    }

    public sealed class CommonsActivityImageService
    {
        private static readonly HttpClient Client = CreateClient();
        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TeachFlex/1.0 (classroom activity sheet image search)");
            return client;
        }

        private static string Meta(JsonElement metadata, string name)
        {
            if (!metadata.TryGetProperty(name, out var entry) ||
                !entry.TryGetProperty("value", out var value)) return "";
            return WebUtility.HtmlDecode(Regex.Replace(value.GetString() ?? "", "<[^>]+>", " ")).Trim();
        }

        public async Task<IReadOnlyList<CommonsImageResult>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<CommonsImageResult>();
            var results = await SearchCoreAsync(query.Trim());
            if (results.Count == 0 && query.Contains(' '))
                results = await SearchCoreAsync(query.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);
            return results;
        }

        private async Task<IReadOnlyList<CommonsImageResult>> SearchCoreAsync(string query)
        {
            string url = "https://commons.wikimedia.org/w/api.php?action=query&format=json&formatversion=2" +
                "&generator=search&gsrnamespace=6&gsrlimit=20&gsrsearch=" + Uri.EscapeDataString(query) +
                "&prop=imageinfo&iiprop=url%7Cextmetadata%7Cmime&iiurlwidth=640";
            using var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var results = new List<CommonsImageResult>();
            if (!doc.RootElement.TryGetProperty("query", out var root) ||
                !root.TryGetProperty("pages", out var pages)) return results;
            foreach (var page in pages.EnumerateArray())
            {
                if (!page.TryGetProperty("imageinfo", out var infoList) || infoList.GetArrayLength() == 0) continue;
                var info = infoList[0];
                string mime = info.TryGetProperty("mime", out var mimeValue) ? mimeValue.GetString() ?? "" : "";
                if (mime is not ("image/jpeg" or "image/png")) continue;
                if (!info.TryGetProperty("thumburl", out var thumbValue) ||
                    !info.TryGetProperty("descriptionurl", out var descriptionValue) ||
                    !info.TryGetProperty("extmetadata", out var metadata)) continue;
                string license = Meta(metadata, "LicenseShortName");
                if (string.IsNullOrWhiteSpace(license)) license = "Check file page";
                string thumb = thumbValue.GetString() ?? "";
                string description = descriptionValue.GetString() ?? "";
                if (!Trusted(thumb, "upload.wikimedia.org") || !Trusted(description, "commons.wikimedia.org")) continue;
                results.Add(new CommonsImageResult
                {
                    Title = page.GetProperty("title").GetString() ?? "Image",
                    PageUrl = description, ThumbnailUrl = thumb,
                    License = license, LicenseUrl = Meta(metadata, "LicenseUrl"),
                    Author = Meta(metadata, "Artist")
                });
            }
            return results;
        }

        private static bool Trusted(string address, string host) =>
            Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps && uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase);

        public async Task<byte[]> DownloadAsync(CommonsImageResult image)
        {
            if (!Trusted(image.ThumbnailUrl, "upload.wikimedia.org"))
                throw new InvalidOperationException("Invalid image source.");
            using var response = await Client.GetAsync(image.ThumbnailUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is > 3_000_000)
                throw new InvalidOperationException("Image is too large. Select another image.");
            string mime = response.Content.Headers.ContentType?.MediaType ?? "";
            if (mime is not ("image/jpeg" or "image/png"))
                throw new InvalidOperationException("Only JPG and PNG images are supported.");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length > 3_000_000) throw new InvalidOperationException("Image is too large.");
            return bytes;
        }
    }
}
