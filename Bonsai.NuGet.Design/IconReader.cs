using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace Bonsai.NuGet.Design
{
    class IconReader
    {
        static readonly HttpClient httpClient = new();
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");
        static readonly TimeSpan DefaultIconTimeout = TimeSpan.FromSeconds(10);
        static readonly Image DefaultIconImage = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        readonly ConcurrentDictionary<Uri, Lazy<Task<Image>>> iconCache = new();
        readonly Task<Image> defaultIcon;
        readonly Size targetSize;

        public IconReader(Size size)
        {
            targetSize = size;
            defaultIcon = GetAsync(PackageDefaultIconUrl);
        }

        public void ClearCache()
        {
            iconCache.Clear();
        }

        public Task<Image> GetDefaultIconAsync() => defaultIcon;

        public Task<Image> GetAsync(Uri iconUrl, CancellationToken cancellationToken = default)
        {
            if (iconUrl == null) return defaultIcon;
            if (!iconCache.TryGetValue(iconUrl, out Lazy<Task<Image>> result))
            {
                var iconStream = new Lazy<Task<Image>>(() => iconUrl.IsFile
                    ? GetFileRequestAsync(iconUrl, cancellationToken)
                    : GetWebRequestAsync(iconUrl, defaultIcon, cancellationToken));
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
                    return ResizeImage(image, targetSize);
                }
            }
            catch (IOException) { } // fallback if error reading icon stream
            catch (ArgumentException) { } // fallback if invalid path or image data
            catch (UnauthorizedAccessException) { } // fallback if unauthorized access

            return await defaultIcon;
        }

        async Task<Image> GetWebRequestAsync(Uri requestUri, Task<Image> fallbackImage = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestCancellation.CancelAfter(DefaultIconTimeout);

                using var response = await httpClient.GetAsync(requestUri, requestCancellation.Token);
                if (response.IsSuccessStatusCode)
                {
                    var mediaType = response.Content.Headers.ContentType.MediaType;
                    if (mediaType.StartsWith("image/") || mediaType.StartsWith("application/octet-stream"))
                    {
                        var responseStream = await response.Content.ReadAsStreamAsync();
                        try
                        {
                            using var image = Image.FromStream(responseStream);
                            return ResizeImage(image, targetSize);
                        }
                        catch (ArgumentException) { } // fallback if decoding image fails
                    }
                }
            }
            catch (HttpRequestException) { } // fallback if request fails
            return fallbackImage != null
                ? await fallbackImage
                : DefaultIconImage;
        }

        static Bitmap ResizeImage(Image image, Size newSize)
        {
            var result = new Bitmap(newSize.Width, newSize.Height);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
            }
            return result;
        }
    }
}
