using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bonsai.Editor
{
    static class DocumentationHelper
    {
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

            WebException lastException = default;
            for (int i = 0; i < hrefs.Length; i++)
            {
                try { return await GetXRefMapAsync($"{baseUrl}{hrefs[i]}"); }
                catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse &&
                                              httpResponse.StatusCode is HttpStatusCode.NotFound)
                {
                    lastException = ex;
                    continue;
                }
            }

            throw lastException;
        }

        static async Task<Dictionary<string, string>> GetXRefMapAsync(string baseUrl)
        {
            var requestUrl = $"{baseUrl}/xrefmap.yml";
            var request = WebRequest.CreateHttp(requestUrl);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
            using var response = await request.GetResponseAsync();
            var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var xrefmap = deserializer.Deserialize<XRefMap>(reader);
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
