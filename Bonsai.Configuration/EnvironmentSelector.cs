﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bonsai.Configuration.Properties;
using Bonsai.NuGet;
using NuGet.Common;
using NuGet.Versioning;

namespace Bonsai.Configuration;

public static class EnvironmentSelector
{
    internal const string BonsaiName = "Bonsai";
    internal const string BonsaiConfig = "Bonsai.config";
    internal const string NuGetConfig = "NuGet.config";
    const string BonsaiPortableUrl = "https://github.com/bonsai-rx/bonsai/releases/download/{0}/Bonsai.zip";
    static readonly SHA256 sha256 = SHA256.Create();
    static readonly HttpClient httpClient = GetHttpClient();
    static readonly BootstrapperInfo defaultBootstrapper = GetDefaultBootstrapper();
    static readonly Dictionary<string, string> knownChecksums = new()
    {
        { "2.8.5", "30293da62cf6df08581235b5d9a468c9005007bf4b6315b8a79eedc34080f192" },
        { "2.8.4", "ee63d29dd6eabf5743019ed91ed2319855a88fd4725608853cb0d277a2ef96bc" },
        { "2.8.3", "db68236020581cd8835033de468c60619e6ba3d3e0a868ededb2a6b766f4914b" },
        { "2.8.2", "7a54b870d50af0dc7d3cbcaeee7fef1e62e460baaf466df06ad2d8cc90912ad5" },
        { "2.8.1", "36b776ddb76a13a05ebc06cc73e7b6ef46c392a4b7bd073d5dde7d2e773876cc" },
        { "2.8.0", "e384ba8e964bb580fa001609cc17ecc24f9d62f445adcf74705de6e45f1aa618" },
        { "2.7.2", "2efb2884096329eb753681c583660cb237e634e6047ca3dd20e0ea208e0d868f" },
        { "2.7.1", "7f3b931e0133b34e9af25b207f678348ba00aa88f8f2552151e78b32097fd2a3" },
        { "2.7.0", "b423a7b81ddf5171133321929a931f41bab743f39d0ff8c01346347551d71ab9" },
        { "2.6.3", "870459b277f3a28b3813971b4353695f77d682288852edd42ef39e013e6f37bf" },
        { "2.6.2", "f5f1cc842800a5a44bff556a277be5b1eb75e662d146c9144d85afaa115d4953" },
        { "2.6.1", "89ea9de43cfdde0bcb77c6c5ce4a927df9faaed877b41364c131daf22a7b58ea" },
        { "2.6.0", "807d99fe82511dd7362ff0eee7d997ce4c196894d8471eff4993707fa5e8378c" },
        { defaultBootstrapper.Version, defaultBootstrapper.Checksum }
    };

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

    static BootstrapperInfo GetDefaultBootstrapper()
    {
        BootstrapperInfo bootstrapper;
#if NETFRAMEWORK
        var launcherPath = Assembly.GetEntryAssembly().Location;
#else
        var launcherPath = Environment.ProcessPath;
#endif
        bootstrapper.Path = launcherPath;
        bootstrapper.Checksum = GetFileChecksum(bootstrapper.Path);
        bootstrapper.Version = NuGetVersion.Parse(Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion)
            .ToString();
        return bootstrapper;
    }

    public static string CreateLocalBootstrapper()
    {
        var environmentConfigPath = Path.GetFullPath(Path.Combine(Constants.BonsaiExtension, BonsaiConfig));
        if (File.Exists(environmentConfigPath))
            throw new InvalidOperationException($"The file '{environmentConfigPath}' already exists.");

        var bootstrapperDirectory = Directory.CreateDirectory(Path.GetDirectoryName(environmentConfigPath));
        return TryInitializeLocalBootstrapper(bootstrapperDirectory.FullName);
    }

    public static bool TryGetLocalBootstrapper(string path, out BootstrapperInfo bootstrapper)
    {
        if (string.IsNullOrEmpty(path))
            path = Environment.CurrentDirectory;
        else if (!Directory.Exists(path))
            path = Path.GetDirectoryName(path);

        var currentPath = Path.GetFullPath(path);
        var startFolder = Path.GetDirectoryName(defaultBootstrapper.Path);
        while (!string.IsNullOrEmpty(currentPath) && currentPath != startFolder)
        {
            var environmentConfigPath = Path.Combine(currentPath, Constants.BonsaiExtension, BonsaiConfig);
            var bootstrapperPath = Path.ChangeExtension(environmentConfigPath, ".exe");
            if (bootstrapperPath == defaultBootstrapper.Path)
                break;

            try
            {
                var document = XDocument.Load(environmentConfigPath);
                var bootstrapperElement = document
                    .Element("PackageConfiguration")?
                    .Element("Packages")?
                    .Descendants("Package")?
                    .FirstOrDefault(element => element.Attribute("id")?.Value == BonsaiName);
                if (bootstrapperElement is not null &&
                    bootstrapperElement.Attribute("version")?.Value is string version)
                {
                    bootstrapper.Version = version;
                    bootstrapper.Path = bootstrapperPath;
                    knownChecksums.TryGetValue(version, out bootstrapper.Checksum);
                    return true;
                }
            }
            catch { } // ignore if config not found or inaccessible
            currentPath = Path.GetDirectoryName(currentPath);
        }

        bootstrapper = defaultBootstrapper;
        return false;
    }

    static string TryInitializeLocalBootstrapper(string bootstrapperDirectory)
    {
        var bootstrapperPath = Path.Combine(bootstrapperDirectory, Path.GetFileName(defaultBootstrapper.Path));
        File.Copy(defaultBootstrapper.Path, bootstrapperPath);
        try
        {
            var sourceNuGetConfigPath = Path.Combine(Path.GetDirectoryName(defaultBootstrapper.Path), NuGetConfig);
            var bootstrapperNuGetConfigPath = Path.Combine(bootstrapperDirectory, NuGetConfig);
            File.Copy(sourceNuGetConfigPath, bootstrapperNuGetConfigPath);
        }
        catch { } // best effort, ignore if source config does not exist or target already exists
        return bootstrapperPath;
    }

    public static string TryInitializeLocalBootstrapper(BootstrapperInfo bootstrapper)
    {
        try
        {
            if (string.IsNullOrEmpty(bootstrapper.Checksum))
            {
                throw new ApplicationException(string.Format(
                    Resources.Error_UnsupportedBootstrapperVersion,
                    Path.ChangeExtension(bootstrapper.Path, ".config"),
                    bootstrapper.Version,
                    defaultBootstrapper.Version));
            }

            if (bootstrapper.Checksum != GetFileChecksum(bootstrapper.Path))
                throw new InvalidDataException(
                    string.Format(Resources.Error_InvalidBootstrapperChecksum, bootstrapper.Path)
                );

            // Do nothing if bootstrapper already exists and is valid
            return bootstrapper.Path;
        }
        catch (FileNotFoundException) { } // Download only if file not found

        // If version matches default bootstrapper, simply initialize a local environment
        if (bootstrapper.Version == defaultBootstrapper.Version)
        {
            var bootstrapperDirectory = Path.GetDirectoryName(bootstrapper.Path);
            return TryInitializeLocalBootstrapper(bootstrapperDirectory);
        }

        return null;
    }

    public static async Task DownloadBootstrapperExecutableAsync(
        BootstrapperInfo bootstrapper,
        ILogger logger = default,
        Func<IProgressBar> progressBarFactory = default,
        CancellationToken cancellationToken = default)
    {
        var bootstrapperUri = GetPortableDownloadUri(bootstrapper.Version);
        var bootstrapperDirectory = Path.GetDirectoryName(bootstrapper.Path);

        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), BonsaiName, Guid.NewGuid().ToString());
        using var tempDirectory = new TempDirectory(tempDirectoryPath);
        logger?.LogInformation($"Downloading {BonsaiName} {bootstrapper.Version}...");
        var downloadPath = Path.Combine(
            tempDirectory.Path,
            Path.GetFileNameWithoutExtension(bootstrapper.Path) + ".zip");
        await DownloadFileAsync(bootstrapperUri, downloadPath, progressBarFactory, cancellationToken);

        ZipFile.ExtractToDirectory(downloadPath, tempDirectory.Path);
        var tempBootstrapper = Path.ChangeExtension(downloadPath, ".exe");
        if (bootstrapper.Checksum != GetFileChecksum(tempBootstrapper))
            throw new InvalidDataException(
                string.Format(Resources.Error_InvalidDownloadChecksum, $"{BonsaiName} {bootstrapper.Version}")
            );

        File.Move(tempBootstrapper, bootstrapper.Path);
        try
        {
            var tempNuGetConfigPath = Path.Combine(tempDirectory.Path, NuGetConfig);
            var bootstrapperNuGetConfigPath = Path.Combine(bootstrapperDirectory, NuGetConfig);
            File.Move(tempNuGetConfigPath, bootstrapperNuGetConfigPath);
        }
        catch { } // best effort, ignore if source config does not exist or target already exists
    }

    static string GetPortableDownloadUri(string version)
    {
        // account for versions where tag was not semver
        if (version is "2.7.0")
            version = "2.7";
        else if (version is "2.6.0")
            version = "2.6";
        return string.Format(BonsaiPortableUrl, version);
    }

    static string GetFileChecksum(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    static async Task DownloadFileAsync(string uri, string path, Func<IProgressBar> progressBarFactory = default, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        using var contentStream = await response.Content.ReadAsStreamAsync();

        using var progressBar = progressBarFactory?.Invoke();
        var progress = contentLength is not null ? new Progress<long>(
            currentBytesRead =>
            {
                var percent = (int)(100 * (currentBytesRead / (double)contentLength));
                progressBar.Report(percent);
            }) : default;
        await WriteAllBytesAsync(contentStream, path, progress, cancellationToken);
        progressBar.Report(100);
    }

    static async Task WriteAllBytesAsync(
        Stream stream,
        string path,
        IProgress<long> progress = null,
        CancellationToken cancellationToken = default)
    {
        const int BufferSize = 81920;
        var buffer = new byte[BufferSize];
        using var destination = File.OpenWrite(path);

        int bytesRead;
        long totalBytesRead = 0;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }

    public static int RunProcess(string fileName, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = fileName;
        startInfo.Arguments = string.Join(" ", arguments.Select(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg));
        startInfo.WorkingDirectory = Environment.CurrentDirectory;
        startInfo.UseShellExecute = false;

        const int MaxConsoleTitleLength = 24500;
        Console.Title = fileName.Substring(0, Math.Min(fileName.Length, MaxConsoleTitleLength));
        using var process = Process.Start(startInfo);
        process.WaitForExit();
        return process.ExitCode;
    }
}
