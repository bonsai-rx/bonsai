using Bonsai.Design;
using Bonsai.NuGet.Design.Properties;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    class PackageViewController
    {
        const int PackagesPerPage = 10;
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");
        static readonly TimeSpan DefaultIconTimeout = TimeSpan.FromSeconds(10);
        static readonly Image DefaultIconImage = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        readonly ConcurrentDictionary<Uri, IObservable<Image>> iconCache;
        readonly IObservable<Image> defaultIcon;

        bool loaded;
        readonly string packageManagerPath;
        readonly IEnumerable<string> packageTypes;
        readonly PackageSourceProvider packageSourceProvider;
        readonly List<IDisposable> activeRequests;
        PackageQuery packageQuery;
        string feedExceptionMessage;
        IDisposable eventSubscription;
        Form operationDialog;

        readonly Control control;
        readonly PackageView packageView;
        readonly PackageDetails packageDetails;
        readonly PackagePageSelector packagePageSelector;
        readonly ImageList packageIcons;
        readonly CueBannerComboBox searchComboBox;
        readonly CheckBox prereleaseCheckBox;
        readonly Func<bool> getUpdateFeed;
        readonly Action<bool> setMultiOperationVisible;

        public PackageViewController(
            NuGetFramework projectFramework,
            string path,
            Control owner,
            PackageView view,
            PackageDetails details,
            PackagePageSelector pageSelector,
            ImageList icons,
            CueBannerComboBox search,
            CheckBox prerelease,
            Func<bool> updateFeed,
            Action<bool> multiOperationVisible,
            IEnumerable<string> packageTypeFilter)
        {
            ProjectFramework = projectFramework ?? throw new ArgumentNullException(nameof(projectFramework));
            control = owner ?? throw new ArgumentNullException(nameof(owner));
            packageView = view ?? throw new ArgumentNullException(nameof(view));
            packageDetails = details ?? throw new ArgumentNullException(nameof(details));
            packagePageSelector = pageSelector ?? throw new ArgumentNullException(nameof(pageSelector));
            packageIcons = icons ?? throw new ArgumentNullException(nameof(icons));
            searchComboBox = search ?? throw new ArgumentNullException(nameof(search));
            prereleaseCheckBox = prerelease ?? throw new ArgumentNullException(nameof(prerelease));
            getUpdateFeed = updateFeed ?? throw new ArgumentNullException(nameof(updateFeed));
            setMultiOperationVisible = multiOperationVisible ?? throw new ArgumentNullException(nameof(multiOperationVisible));
            packageTypes = packageTypeFilter;
            control.KeyDown += control_KeyDown;
            prereleaseCheckBox.CheckedChanged += prereleaseFilterCheckBox_CheckedChanged;
            packagePageSelector.SelectedIndexChanged += packagePageSelector_SelectedIndexChanged;
            packagePageSelector.Visible = false;

            packageManagerPath = path;
            iconCache = new ConcurrentDictionary<Uri, IObservable<Image>>();
            defaultIcon = GetPackageIcon(PackageDefaultIconUrl);

            activeRequests = new List<IDisposable>();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory, null, machineWideSettings);
            packageSourceProvider = new PackageSourceProvider(settings);
            PackageManager = CreatePackageManager(packageSourceProvider, Enumerable.Empty<PackageManagerPlugin>());
            searchComboBox.CueBanner = Resources.SearchCueBanner;
            packageDetails.ProjectFramework = ProjectFramework;
            packageDetails.PathResolver = PackageManager.PathResolver;
        }

        public string SearchPrefix { get; set; }

        public SourceRepository SelectedRepository { get; set; }

        public NuGetFramework ProjectFramework { get; private set; }

        public LicenseAwarePackageManager PackageManager { get; private set; }

        public void ClearActiveRequests()
        {
            activeRequests.RemoveAll(request =>
            {
                request.Dispose();
                return true;
            });
        }

        LicenseAwarePackageManager CreatePackageManager(PackageSourceProvider packageSourceProvider, IEnumerable<PackageManagerPlugin> plugins)
        {
            var packageManager = new LicenseAwarePackageManager(packageSourceProvider, packageManagerPath);
            packageManager.RequiringLicenseAcceptance += packageManager_RequiringLicenseAcceptance;
            packageManager.Logger = new EventLogger();
            foreach (var plugin in plugins)
            {
                packageManager.PackageManagerPlugins.Add(plugin);
            }
            return packageManager;
        }

        void packageManager_RequiringLicenseAcceptance(object sender, RequiringLicenseAcceptanceEventArgs e)
        {
            if (control.InvokeRequired)
            {
                control.Invoke((EventHandler<RequiringLicenseAcceptanceEventArgs>)packageManager_RequiringLicenseAcceptance, sender, e);
            }
            else
            {
                if (operationDialog == null) return;
                operationDialog.Hide();
                using (var licenseDialog = new LicenseAcceptanceDialog(e.LicensePackages))
                {
                    e.LicenseAccepted = licenseDialog.ShowDialog(control) == DialogResult.Yes;
                    if (e.LicenseAccepted)
                    {
                        operationDialog.Show();
                    }
                }
            }
        }

        public void OnLoad(EventArgs e)
        {
            var selectHandler = SelectPackageDetails()
                .ObserveOn(control)
                .Do(package => packageDetails.SetPackage(package))
                .Select(result => Unit.Default);
            var searchHandler = Observable.FromEventPattern<EventArgs>(
                handler => searchComboBox.TextChanged += new EventHandler(handler),
                handler => searchComboBox.TextChanged -= new EventHandler(handler))
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(control)
                .Do(evt => UpdatePackageQuery())
                .Select(result => Unit.Default);
            eventSubscription = selectHandler.Merge(searchHandler).Subscribe();
            loaded = true;
        }

        public void OnHandleDestroyed(EventArgs e)
        {
            ClearActiveRequests();
            eventSubscription.Dispose();
        }

        bool AllowPrereleaseVersions
        {
            get
            {
                return prereleaseCheckBox.Checked || SelectedRepository == PackageManager.LocalRepository;
            }
        }

        public void UpdatePackageQuery()
        {
            iconCache.Clear();
            var prefix = SearchPrefix;
            var searchTerm = searchComboBox.Text;
            if (!string.IsNullOrEmpty(prefix)) searchTerm = prefix + searchTerm;
            packageQuery = new PackageQuery(searchTerm, PackagesPerPage, GetPackageQuery(searchTerm));
            packagePageSelector.SelectedPage = 0;
        }

        QueryContinuation<IEnumerable<IPackageSearchMetadata>> GetPackageQuery(string searchTerm)
        {
            if (PackageManager == null)
            {
                return null;
            }

            var selectedRepository = SelectedRepository;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            var updateFeed = getUpdateFeed();
            if (selectedRepository == null)
            {
                var repositories = PackageManager.SourceRepositoryProvider.GetRepositories();
                var packageQueries = repositories.Select(repository => GetPackageQuery(repository, searchTerm, allowPrereleaseVersions, updateFeed)).ToList();
                if (packageQueries.Count == 1) return packageQueries[0];
                else return AggregateQuery.Create(packageQueries, results => results.SelectMany(xs => xs));
            }

            return GetPackageQuery(selectedRepository, searchTerm, allowPrereleaseVersions, updateFeed);
        }

        QueryContinuation<IEnumerable<IPackageSearchMetadata>> GetPackageQuery(SourceRepository repository, string searchTerm, bool includePrerelease, bool updateFeed)
        {
            if (updateFeed)
            {
                var localPackages = PackageManager.LocalRepository.GetLocalPackages();
                return new UpdateQuery(repository, localPackages, includePrerelease);
            }
            else return new SearchQuery(repository, searchTerm, PackagesPerPage, includePrerelease, packageTypes);
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

        IObservable<Image> GetPackageIconFileRequest(Uri iconUrl)
        {
            return Observable.Defer(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(iconUrl.Fragment)) return defaultIcon;
                    using var packageReader = new PackageArchiveReader(iconUrl.AbsolutePath);
                    using var iconStream = packageReader.GetStream(iconUrl.Fragment.Substring(1));
                    using var image = Image.FromStream(iconStream);
                    return Observable.Return(ResizeImage(image, packageIcons.ImageSize));
                }
                catch (IOException) { return defaultIcon; }
                catch (ArgumentException) { return defaultIcon; }
                catch (UnauthorizedAccessException) { return defaultIcon; }
            });
        }

        IObservable<Image> GetPackageIconWebRequest(Uri iconUrl)
        {
            WebRequest imageRequest;
            try { imageRequest = WebRequest.Create(iconUrl); }
            catch (InvalidOperationException) { return defaultIcon; }
            return (from response in Observable.Defer(() => imageRequest.GetResponseAsync().ToObservable())
                    from image in Observable.If(
                        () => response.ContentType.StartsWith("image/") ||
                            response.ContentType.StartsWith("application/octet-stream"),
                        Observable.Using(
                            () => response.GetResponseStream(),
                            stream =>
                            {
                                try
                                {
                                    using var image = Image.FromStream(stream);
                                    return Observable.Return(ResizeImage(image, packageIcons.ImageSize));
                                }
                                catch (ArgumentException) { return defaultIcon; }
                            }),
                        defaultIcon)
                    select image)
                    .Catch<Image, WebException>(ex => defaultIcon)
                    .Timeout(DefaultIconTimeout, defaultIcon ?? Observable.Return(DefaultIconImage));
        }

        IObservable<Image> GetPackageIcon(Uri iconUrl)
        {
            if (iconUrl == null) return defaultIcon;
            if (!iconCache.TryGetValue(iconUrl, out IObservable<Image> result))
            {
                var iconStream = (iconUrl.IsFile
                    ? GetPackageIconFileRequest(iconUrl)
                    : GetPackageIconWebRequest(iconUrl))
                    .PublishLast();
                result = iconCache.GetOrAdd(iconUrl, iconStream);
                if (iconStream == result)
                {
                    var iconRequest = iconStream.Connect();
                    if (defaultIcon != null) activeRequests.Add(iconRequest);
                }
            }

            return result;
        }

        public void SetPackageViewStatus(string text, Image image = null)
        {
            if (packageView.Nodes.ContainsKey(text)) return;
            setMultiOperationVisible(false);
            packageView.CanSelectNodes = false;
            packageView.BeginUpdate();
            packageView.Nodes.Clear();
            packageIcons.Images.Clear();
            var imageIndex = -1;
            if (image != null)
            {
                packageIcons.Images.Add(image);
                imageIndex = 0;
            }
            packageView.Nodes.Add(text, text, imageIndex, imageIndex);
            packageDetails.SetPackage(null);
            packageView.EndUpdate();
        }

        private void AddPackageRange(IList<IPackageSearchMetadata> packages)
        {
            if (packages.Count > 0)
            {
                if (packages.Count > 1 && packagePageSelector.SelectedPage == 0 &&
                    packageView.OperationText == Resources.UpdateOperationName)
                {
                    setMultiOperationVisible(true);
                }

                packageView.BeginUpdate();
                packageView.Nodes.Clear();
                packageIcons.Images.Clear();
                foreach (var package in packages)
                {
                    AddPackage(package);
                }
                packageView.EndUpdate();
                packageView.CanSelectNodes = true;
            }
        }

        private void AddPackage(IPackageSearchMetadata package)
        {
            var installCheck = false;
            if (SelectedRepository != PackageManager.LocalRepository &&
                packageView.OperationText != Resources.UpdateOperationName)
            {
                var installedPackage = PackageManager.LocalRepository.FindLocalPackage(package.Identity.Id);
                installCheck = installedPackage != null && installedPackage.Identity.Version >= package.Identity.Version;
            }

            var nodeTitle = !string.IsNullOrWhiteSpace(package.Title) ? package.Title : package.Identity.Id;
            var nodeText = string.Join(
                Environment.NewLine, nodeTitle,
                package.Summary ?? package.Description.Split(
                    new[] { Environment.NewLine, "\n", "\r" },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
            var node = packageView.Nodes.Add(package.Identity.Id, nodeText);
            node.Checked = installCheck;
            node.Tag = package;

            var requestIcon = GetPackageIcon(package.IconUrl);
            var iconRequest = requestIcon.ObserveOn(control).Subscribe(image =>
            {
                if (packageIcons.Images.Count == 0)
                {
                    var defaultImage = defaultIcon.Wait();
                    packageIcons.Images.Add(defaultImage);
                }
                packageIcons.Images.Add(package.Identity.Id, image);
                node.ImageKey = package.Identity.Id;
                node.SelectedImageKey = package.Identity.Id;
            });

            activeRequests.Add(iconRequest);
        }

        public void UpdatePackagePage(int pageIndex = 0)
        {
            ClearActiveRequests();
            SetPackageViewStatus(Resources.RetrievingInformationLabel, Resources.WaitImage);

            var query = packageQuery;
            if (!query.HasQueryPage(pageIndex)) packagePageSelector.ShowNext = false;
            var feedRequest = Observable.FromAsync(token => query.GetPackageFeed(pageIndex, token))
                .SelectMany(packages => packages)
                .Catch<IPackageSearchMetadata, InvalidOperationException>(ex =>
                {
                    Exception innerException = ex;
                    feedExceptionMessage = ex.Message;
                    while (innerException.InnerException != null)
                    {
                        innerException = innerException.InnerException;
                        feedExceptionMessage += Environment.NewLine + "\t --> " + innerException.Message;
                    }
                    return Observable.Empty<IPackageSearchMetadata>();
                })
                .ToList()
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(control)
                .Do(packages => AddPackageRange(packages))
                .Sum(packages => packages.Count)
                .Subscribe(packageCount =>
                {
                    packagePageSelector.Visible = query.HasQueryPage(1) || !packageQuery.IsCompleted;
                    packagePageSelector.ShowNext = !packageQuery.IsCompleted || query.HasQueryPage(pageIndex + 1);
                    if (packageCount == 0)
                    {
                        if (feedExceptionMessage != null) SetPackageViewStatus(feedExceptionMessage);
                        else if (packageView.OperationText == Resources.UpdateOperationName)
                        {
                            SetPackageViewStatus(Resources.NoUpdatesAvailableLabel);
                        }
                        else SetPackageViewStatus(Resources.NoItemsFoundLabel);
                    }
                });

            activeRequests.Add(feedRequest);
        }

        public void RunPackageOperation(IEnumerable<IPackageSearchMetadata> packages, bool handleDependencies)
        {
            using (var dialog = new PackageOperationDialog())
            {
                var logger = PackageManager.Logger;
                dialog.RegisterEventLogger((EventLogger)logger);

                IObservable<Unit> operation;
                var uninstallOperation = SelectedRepository == PackageManager.LocalRepository;
                var update = packageView.OperationText == Resources.UpdateOperationName;
                if (uninstallOperation)
                {
                    operation = Observable.FromAsync(async token =>
                    {
                        foreach (var package in packages)
                        {
                            await PackageManager.UninstallPackageAsync(package.Identity, ProjectFramework, handleDependencies, token);
                        }
                    });
                    dialog.Text = Resources.UninstallOperationLabel;
                }
                else
                {
                    var allowPrereleaseVersions = AllowPrereleaseVersions;
                    dialog.Text = update ? Resources.UpdateOperationLabel : Resources.InstallOperationLabel;

                    operation = Observable.FromAsync(async token =>
                    {
                        foreach (var package in packages)
                        {
                            await PackageManager.InstallPackageAsync(package.Identity, ProjectFramework, !handleDependencies, token);
                        }
                    });
                }

                operationDialog = dialog;
                try
                {
                    dialog.Shown += delegate
                    {
                        operation.ObserveOn(control).Subscribe(
                            xs => { },
                            ex => logger.LogError(ex.Message),
                            dialog.Complete);
                    };

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (uninstallOperation || update) UpdatePackageQuery();
                        else UpdatePackagePage(packagePageSelector.SelectedPage);
                    }
                }
                finally { operationDialog = null; }
            }
        }

        public void OnResizeBegin(EventArgs e)
        {
            packageView.BeginUpdate();
        }

        public void OnResizeEnd(EventArgs e)
        {
            packageView.EndUpdate();
            packageView.Refresh();
        }

        private void control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L && e.Modifiers == Keys.Control && !searchComboBox.Focused)
            {
                searchComboBox.Select();
            }
        }

        private IObservable<IPackageSearchMetadata> SelectPackageDetails()
        {
            return Observable.FromEventPattern<TreeViewEventHandler, TreeViewEventArgs>(
                handler => packageView.AfterSelect += handler,
                handler => packageView.AfterSelect -= handler)
                .Select(evt => Observable.StartAsync(async token =>
                {
                    var selectedRepository = SelectedRepository;
                    var package = (IPackageSearchMetadata)evt.EventArgs.Node.Tag;
                    if (package == null) return null;
                    var repositories = selectedRepository == null ? PackageManager.SourceRepositoryProvider.GetRepositories() : new[] { selectedRepository };
                    using (var cacheContext = new SourceCacheContext())
                    {
                        foreach (var repository in repositories)
                        {
                            var metadata = await repository.GetMetadataAsync(package.Identity, cacheContext);
                            if (metadata != null)
                            {
                                var result = (PackageSearchMetadataBuilder.ClonedPackageSearchMetadata)PackageSearchMetadataBuilder.FromMetadata(metadata).Build();
                                result.DownloadCount = package.DownloadCount;
                                return result;
                            }
                        }

                        return null;
                    }
                })).Switch();
        }

        private void prereleaseFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                UpdatePackageQuery();
            }
        }

        private void packagePageSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePackagePage(packagePageSelector.SelectedPage);
        }

        public DialogResult ShowPackageSourceConfigurationDialog()
        {
            using (var dialog = new PackageSourceConfigurationDialog(packageSourceProvider))
            {
                var result = dialog.ShowDialog(control);
                if (result == DialogResult.OK)
                {
                    SelectedRepository = null;
                    feedExceptionMessage = null;
                    PackageManager = CreatePackageManager(packageSourceProvider, PackageManager.PackageManagerPlugins);
                }
                return result;
            }
        }
    }
}
