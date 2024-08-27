using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.NuGet.Design.Properties;
using NuGet.Packaging;

namespace Bonsai.NuGet.Design
{
    class IconReader
    {
        static readonly HttpClient httpClient = GetHttpClient();
        readonly ConcurrentDictionary<Uri, Lazy<Task<Image>>> iconCache = new();
        readonly Task<Image> defaultIconTask;
        readonly Size targetSize;

        public IconReader(Size size)
        {
            targetSize = size;
            DefaultIcon = Resources.PackageDefaultIcon.Resize(targetSize);
            defaultIconTask = Task.FromResult(DefaultIcon);
        }

        static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new() { MustRevalidate = true };
            client.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        public void ClearCache()
        {
            iconCache.Clear();
        }

        public Image DefaultIcon { get; }

        public Task<Image> GetAsync(Uri iconUrl, CancellationToken cancellationToken = default)
        {
            if (iconUrl == null) return defaultIconTask;
            if (!iconCache.TryGetValue(iconUrl, out Lazy<Task<Image>> result))
            {
                var iconStream = new Lazy<Task<Image>>(() => iconUrl.IsFile
                    ? GetFileRequestAsync(iconUrl, cancellationToken)
                    : GetWebRequestAsync(iconUrl, cancellationToken));
                result = iconCache.GetOrAdd(iconUrl, iconStream);
            }

            return result.Value;
        }

        async Task<Image> GetFileRequestAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(requestUri.Fragment))
                {
                    using var packageReader = new PackageArchiveReader(requestUri.AbsolutePath);
                    using var iconStream = await packageReader.GetStreamAsync(requestUri.Fragment.Substring(1), cancellationToken);
                    using var image = Image.FromStream(iconStream);
                    return image.Resize(targetSize);
                }
            }
            catch (IOException) { } // fallback if error reading icon stream
            catch (ArgumentException) { } // fallback if invalid path or image data
            catch (UnauthorizedAccessException) { } // fallback if unauthorized access

            return await defaultIconTask;
        }

        async Task<Image> GetWebRequestAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.GetAsync(requestUri, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var mediaType = response.Content.Headers.ContentType.MediaType;
                    if (mediaType.StartsWith("image/") || mediaType.StartsWith("application/octet-stream"))
                    {
                        var responseStream = await response.Content.ReadAsStreamAsync();
                        try
                        {
                            using var image = Image.FromStream(responseStream);
                            return image.Resize(targetSize);
                        }
                        catch (ArgumentException) { } // fallback if decoding image fails
                    }
                }
            }
            catch (HttpRequestException) { } // fallback if request fails
            return await defaultIconTask;
        }
    }
}
