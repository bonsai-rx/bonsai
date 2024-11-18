using Bonsai.Design;
using Bonsai.NuGet.Design.Properties;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        readonly IconReader iconReader;

        bool loaded;
        readonly string packageManagerPath;
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
            Action<bool> multiOperationVisible)
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
            control.KeyDown += control_KeyDown;
            packageDetails.PackageLinkClicked += packageDetails_PackageLinkClicked;
            prereleaseCheckBox.CheckedChanged += prereleaseFilterCheckBox_CheckedChanged;
            packagePageSelector.SelectedIndexChanged += packagePageSelector_SelectedIndexChanged;
            packagePageSelector.Visible = false;

            packageManagerPath = path;
            iconReader = new IconReader(packageIcons.ImageSize);
            if (packageIcons.Images.Count == 0)
            {
                packageIcons.Images.Add(iconReader.DefaultIcon);
            }

            activeRequests = new List<IDisposable>();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory, null, machineWideSettings);
            packageSourceProvider = new PackageSourceProvider(settings);
            PackageManager = CreatePackageManager(packageSourceProvider, Enumerable.Empty<PackageManagerPlugin>());
            searchComboBox.CueBanner = Resources.SearchCueBanner;
            packageDetails.ProjectFramework = ProjectFramework;
            Operation = packageView.Operation;
        }

        public string SearchPrefix { get; set; }

        public IEnumerable<string> PackageTypes { get; set; }

        public SourceRepository SelectedRepository { get; set; }

        public NuGetFramework ProjectFramework { get; private set; }

        public LicenseAwarePackageManager PackageManager { get; private set; }

        public PackageOperationType Operation
        {
            get => packageView.Operation;
            set => packageView.Operation = value;
        }

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
                .Do(item => packageDetails.SetPackage(item))
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
            iconReader.ClearCache();
            var prefix = SearchPrefix;
            var updateFeed = getUpdateFeed();
            var searchTerm = searchComboBox.Text;
            var pageSize = updateFeed || SelectedRepository == PackageManager.LocalRepository ? int.MaxValue - 1 : PackagesPerPage;
            if (!string.IsNullOrEmpty(prefix)) searchTerm = prefix + searchTerm;
            packageQuery = new PackageQuery(searchTerm, pageSize, GetPackageQuery(searchTerm, pageSize, updateFeed));
            packagePageSelector.SelectedPage = 0;
        }

        QueryContinuation<IEnumerable<IPackageSearchMetadata>> GetPackageQuery(string searchTerm, int pageSize, bool updateFeed)
        {
            if (PackageManager == null)
            {
                return null;
            }

            var selectedRepository = SelectedRepository;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            if (selectedRepository == null)
            {
                var repositories = PackageManager.SourceRepositoryProvider.GetRepositories();
                var packageQueries = repositories.Select(repository => GetPackageQuery(repository, searchTerm, pageSize, allowPrereleaseVersions, updateFeed)).ToList();
                if (packageQueries.Count == 1) return packageQueries[0];
                else return AggregateQuery.Create(packageQueries, results => results.SelectMany(xs => xs));
            }

            return GetPackageQuery(selectedRepository, searchTerm, pageSize, allowPrereleaseVersions, updateFeed);
        }

        QueryContinuation<IEnumerable<IPackageSearchMetadata>> GetPackageQuery(SourceRepository repository, string searchTerm, int pageSize, bool includePrerelease, bool updateFeed)
        {
            return updateFeed
                ? new UpdateQuery(repository, PackageManager.LocalRepository, searchTerm, includePrerelease, PackageTypes)
                : new SearchQuery(repository, searchTerm, pageSize, includePrerelease, PackageTypes);
        }

        IObservable<Image> GetPackageIcon(Uri iconUrl)
        {
            return iconReader.GetAsync(iconUrl).ToObservable();
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
                    packageView.Operation == PackageOperationType.Update)
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
            var installedPackage = PackageManager.LocalRepository.FindLocalPackage(package.Identity.Id);
            var nodeTitle = package.Identity.Id;
            var nodeText = string.Join(
                Environment.NewLine, nodeTitle,
                package.Summary ?? package.Description.Split(
                    new[] { Environment.NewLine, "\n", "\r" },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
            var node = packageView.Nodes.Add(package.Identity.Id, nodeText);
            node.Tag = package;

            if (installedPackage != null)
            {
                var installedPackageNode = node.Nodes.Add(Resources.UpdatesNodeName, installedPackage.Identity.Version.ToString());
                installedPackageNode.Tag = installedPackage;
            }

            var deprecationMetadata = package.GetDeprecationMetadataAsync().Result;
            if (deprecationMetadata != null)
            {
                var deprecationMetadataNode = node.Nodes.Add(Resources.PackageWarningKey, deprecationMetadata.Message);
                deprecationMetadataNode.Tag = deprecationMetadata;
            }

            var requestIcon = GetPackageIcon(package.IconUrl);
            var iconRequest = requestIcon.ObserveOn(control).Subscribe(image =>
            {
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
                        else if (packageView.Operation == PackageOperationType.Update)
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
                var update = packageView.Operation == PackageOperationType.Update;
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

        private void packageDetails_PackageLinkClicked(object sender, PackageSearchEventArgs e)
        {
            searchComboBox.Text = e.SearchTerm;
        }

        private IObservable<PackageViewItem> SelectPackageDetails()
        {
            return Observable.FromEventPattern<TreeViewEventHandler, TreeViewEventArgs>(
                handler => packageView.AfterSelect += handler,
                handler => packageView.AfterSelect -= handler)
                .Select(evt => Observable.StartAsync(async token =>
                {
                    var selectedNode = evt.EventArgs.Node;
                    var selectedRepository = SelectedRepository;
                    var selectedPackage = (IPackageSearchMetadata)selectedNode.Tag;
                    if (selectedPackage == null) return null;

                    var localPackageNode = selectedNode.Nodes.Count > 0 ? selectedNode.Nodes[Resources.UpdatesNodeName] : null;
                    var localPackage = (LocalPackageInfo)localPackageNode?.Tag;

                    var repositories = selectedRepository == null ? PackageManager.SourceRepositoryProvider.GetRepositories() : new[] { selectedRepository };
                    using (var cacheContext = new SourceCacheContext())
                    {
                        foreach (var repository in repositories)
                        {
                            var metadata = await repository.GetMetadataAsync(selectedPackage.Identity, cacheContext);
                            if (metadata != null)
                            {
                                var imageIndex = packageView.ImageList.Images.IndexOfKey(selectedNode.ImageKey);
                                var result = (PackageSearchMetadataBuilder.ClonedPackageSearchMetadata)PackageSearchMetadataBuilder.FromMetadata(metadata).Build();
                                var packageVersions = await selectedPackage.GetVersionsAsync();
                                var deprecationMetadata = await result.GetDeprecationMetadataAsync();
                                result.PrefixReserved = selectedPackage.PrefixReserved;
                                result.DownloadCount = selectedPackage.DownloadCount;
                                return new PackageViewItem(
                                    selectedPackage: result,
                                    packageVersions,
                                    deprecationMetadata,
                                    repository,
                                    localPackage,
                                    packageView.ImageList,
                                    imageIndex);
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
