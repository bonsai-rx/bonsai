using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bonsai.Editor
{
    static class DocumentationHelper
    {
        static readonly HttpClient httpClient = GetHttpClient();

        static HttpClient GetHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate //TODO: Use DecompressionMethods.All upon .NET modernization
            });
            client.DefaultRequestHeaders.CacheControl = new() { MustRevalidate = true };
            client.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        public static async Task<Uri> GetDocumentationAsync(this IDocumentationProvider provider, string assemblyName, string uid)
        {
            var baseUrl = provider.GetDocumentationUrl(assemblyName);
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException($"No documentation found for the specified module.", nameof(assemblyName));
            }

            return await GetDocumentationAsync(baseUrl, uid);
        }

        static async Task<Uri> GetDocumentationAsync(string baseUrl, string uid)
        {
            var lookup = await GetXRefMapAsync(baseUrl.TrimEnd('/'), "/docs", string.Empty);
            return new Uri(lookup[uid]);
        }

        static async Task<Dictionary<string, string>> GetXRefMapAsync(string baseUrl, params string[] hrefs)
        {
            if (hrefs == null || hrefs.Length == 0)
            {
                throw new ArgumentException("No downstream URLs have been specified.", nameof(hrefs));
            }

            Exception lastException = default;
            for (int i = 0; i < hrefs.Length; i++)
            {
                try { return await GetXRefMapAsync($"{baseUrl}{hrefs[i]}"); }
                catch (HttpRequestException ex)
                { lastException ??= ex; } // Always prefer a DocumentationException as it'll be more specific
                catch (DocumentationException ex)
                { lastException = ex; }
            }

            throw lastException;
        }

        static async Task<Dictionary<string, string>> GetXRefMapAsync(string baseUrl)
        {
            var requestUrl = $"{baseUrl}/xrefmap.yml";
            using var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            if (reader.ReadLine().Trim() != "### YamlMime:XRefMap")
                throw new DocumentationException("The documentation server did not respond with a cross-reference map.");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            XRefMap xrefmap;
            try { xrefmap = deserializer.Deserialize<XRefMap>(reader); }
            catch (YamlException ex)
            {
                throw new DocumentationException("The cross-reference map returned by the documentation server is malformed.", ex);
            }

            if (xrefmap.References is null)
                throw new DocumentationException("The cross-reference map returned by the documentation server is malformed.");

            return xrefmap.References.ToDictionary(
                reference => reference.Uid,
                reference => $"{baseUrl}/{reference.Href}");
        }

        class XRefMap
        {
            public bool? Sorted { get; set; }

            public List<XRefSpec> References { get; set; }
        }

        class XRefSpec
        {
            public string Uid { get; set; }

            public string Name { get; set; }

            public string Href { get; set; }

            public string CommentId { get; set; }

            public string FullName { get; set; }

            public string NameWithType { get; set; }

            public bool? IsSpec { get; set; }
        }
    }
}
