﻿using Bonsai.Design;
using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageManagerDialog : Form, IPackageManager
    {
        const int PackagesPerPage = 10;
        const string SortByMostDownloads = "Most Downloads";
        const string SortByPublishedDate = "Published Date";
        const string SortByNameAscending = "Name: Ascending";
        const string SortByNameDescending = "Name: Descending";
        const string SortByRelevance = "Relevance";
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");
        static readonly TimeSpan DefaultIconTimeout = TimeSpan.FromSeconds(10);
        static readonly Image DefaultIconImage = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        readonly ConcurrentDictionary<Uri, IObservable<Image>> iconCache;
        readonly IObservable<Image> defaultIcon;

        bool loaded;
        readonly string packageManagerPath;
        readonly IPackageSourceProvider packageSourceProvider;
        Dictionary<string, PackageManager> packageManagers;
        PackageManager selectedManager;
        IPackageRepository selectedRepository;
        string feedExceptionMessage;
        List<IDisposable> activeRequests;
        IDisposable searchSubscription;
        Form operationDialog;

        TreeNode installedPackagesNode;
        TreeNode onlineNode;
        TreeNode updatesNode;
        TreeNode collapsingNode;
        TreeNode selectingNode;

        public PackageManagerDialog(string path)
        {
            InitializeComponent();
            packageManagerPath = path;
            iconCache = new ConcurrentDictionary<Uri, IObservable<Image>>();
            defaultIcon = GetPackageIcon(PackageDefaultIconUrl);

            activeRequests = new List<IDisposable>();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(null, null, machineWideSettings);
            packageSourceProvider = new PackageSourceProvider(settings);
            packageManagers = CreatePackageManagers();
            InitializeRepositoryViewNodes();
            multiOperationPanel.Visible = false;
            searchComboBox.CueBanner = Resources.SearchOnlineCueBanner;
            searchComboBox.Select();

            sortComboBox.Items.Add(SortByMostDownloads);
            sortComboBox.Items.Add(SortByPublishedDate);
            sortComboBox.Items.Add(SortByNameAscending);
            sortComboBox.Items.Add(SortByNameDescending);
            sortComboBox.SelectedIndex = 0;
            releaseFilterComboBox.SelectedIndex = 0;
        }

        void ClearActiveRequests()
        {
            iconCache.Clear();
            activeRequests.RemoveAll(request =>
            {
                request.Dispose();
                return true;
            });
        }

        PackageManager CreatePackageManager(IPackageRepository sourceRepository, EventLogger logger)
        {
            var packageManager = new LicenseAwarePackageManager(sourceRepository, packageManagerPath);
            packageManager.Logger = logger;
            packageManager.PackageInstalled += (sender, e) => OnPackageInstalled(e);
            packageManager.PackageInstalling += (sender, e) => OnPackageInstalling(e);
            packageManager.PackageUninstalled += (sender, e) => OnPackageUninstalled(e);
            packageManager.PackageUninstalling += (sender, e) => OnPackageUninstalling(e);
            packageManager.RequiringLicenseAcceptance += packageManager_RequiringLicenseAcceptance;
            return packageManager;
        }

        void packageManager_RequiringLicenseAcceptance(object sender, RequiringLicenseAcceptanceEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((EventHandler<RequiringLicenseAcceptanceEventArgs>)packageManager_RequiringLicenseAcceptance, sender, e);
            }
            else
            {
                if (operationDialog == null) return;
                operationDialog.Hide();
                using (var licenseDialog = new LicenseAcceptanceDialog(e.LicensePackages))
                {
                    e.LicenseAccepted = licenseDialog.ShowDialog(this) == DialogResult.Yes;
                    if (e.LicenseAccepted)
                    {
                        operationDialog.Show();
                    }
                }
            }
        }

        private Dictionary<string, PackageManager> CreatePackageManagers()
        {
            var logger = new EventLogger();
            var managers = new Dictionary<string, PackageManager>();
            var aggregateRepository = packageSourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, false);
            managers.Add(Resources.AllNodeName, CreatePackageManager(aggregateRepository, logger));
            var packageRepositories = packageSourceProvider
                .GetEnabledPackageSources()
                .Zip(aggregateRepository.Repositories, (source, repository) => new
                {
                    name = source.Name,
                    manager = CreatePackageManager(repository, logger)
                });

            foreach (var repository in packageRepositories)
            {
                managers.Add(repository.name, repository.manager);
            }
            return managers;
        }

        private void InitializeRepositoryViewNode(TreeNode rootNode)
        {
            foreach (var pair in packageManagers)
            {
                var node = rootNode.Nodes.Add(pair.Key);
                node.Tag = pair.Value;
            }
        }

        private void InitializeRepositoryViewNodes()
        {
            repositoriesView.Nodes.Clear();
            installedPackagesNode = repositoriesView.Nodes.Add(Resources.InstalledPackagesNodeName);
            var allInstalledNode = installedPackagesNode.Nodes.Add(Resources.AllNodeName);
            allInstalledNode.Tag = packageManagers[Resources.AllNodeName];

            onlineNode = repositoriesView.Nodes.Add(Resources.OnlineNodeName);
            InitializeRepositoryViewNode(onlineNode);
            onlineNode.Expand();

            updatesNode = repositoriesView.Nodes.Add(Resources.UpdatesNodeName);
            InitializeRepositoryViewNode(updatesNode);
        }

        protected override void OnLoad(EventArgs e)
        {
            searchSubscription = Observable.FromEventPattern<EventArgs>(
                handler => searchComboBox.TextChanged += new EventHandler(handler),
                handler => searchComboBox.TextChanged -= new EventHandler(handler))
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(this)
                .Subscribe(evt =>
                {
                    var searchText = searchComboBox.Text;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        if (!sortComboBox.Items.Contains(SortByRelevance))
                        {
                            sortComboBox.Items.Insert(0, SortByRelevance);
                        }
                        if (!searchComboBox.Items.Contains(searchText))
                        {
                            searchComboBox.Items.Add(searchComboBox.Text);
                        }
                    }
                    else sortComboBox.Items.Remove(SortByRelevance);
                    sortComboBox.SelectedIndex = 0;
                    UpdatePackageFeed();
                });

            loaded = true;
            SelectDefaultNode();
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            const int MaxImageSize = 256;
            packageView.ItemHeight = (int)(64 * factor.Height);
            packageIcons.ImageSize = new Size(
                Math.Min(MaxImageSize, (int)(32 * factor.Height)),
                Math.Min(MaxImageSize, (int)(32 * factor.Height)));
            base.ScaleControl(factor, specified);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ClearActiveRequests();
            searchSubscription.Dispose();
            base.OnHandleDestroyed(e);
        }

        bool AllowPrereleaseVersions
        {
            get
            {
                return releaseFilterComboBox.SelectedIndex == 1 ||
                    selectedRepository == packageManagers[Resources.AllNodeName].LocalRepository;
            }
        }

        void SelectDefaultNode()
        {
            var selectedNode = onlineNode.Nodes
                .Cast<TreeNode>()
                .FirstOrDefault(node => node.Text == BonsaiMachineWideSettings.SettingsName)
                ?? onlineNode.FirstNode;
            repositoriesView.SelectedNode = selectedNode;
            repositoriesView.Select();
        }

        Func<IQueryable<IPackage>> GetPackageFeed()
        {
            var searchTerm = searchComboBox.Text;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            var sortMode = (string)sortComboBox.SelectedItem;
            var updateFeed = updatesNode.IsExpanded;
            return () =>
            {
                if (selectedRepository == null || selectedManager == null)
                {
                    return Enumerable.Empty<IPackage>().AsQueryable();
                }

                IQueryable<IPackage> packages;
                if (updateFeed)
                {
                    var localPackages = selectedManager.LocalRepository.GetPackages();
                    try { packages = selectedRepository.GetUpdates(localPackages, allowPrereleaseVersions, false).AsQueryable(); }
                    catch (AggregateException e) { return Observable.Throw<IPackage>(e.InnerException).ToEnumerable().AsQueryable(); }
                    catch (WebException e) { return Observable.Throw<IPackage>(e).ToEnumerable().AsQueryable(); }
                }
                else
                {
                    try { packages = selectedRepository.Search(searchTerm, allowPrereleaseVersions); }
                    catch (WebException e) { return Observable.Throw<IPackage>(e).ToEnumerable().AsQueryable(); }
                    if (allowPrereleaseVersions) packages = packages.Where(p => p.IsAbsoluteLatestVersion);
                    else packages = packages.Where(p => p.IsLatestVersion);
                }
                switch (sortMode)
                {
                    case SortByRelevance: break;
                    case SortByMostDownloads: packages = packages.OrderByDescending(p => p.DownloadCount); break;
                    case SortByPublishedDate: packages = packages.OrderByDescending(p => p.Published); break;
                    case SortByNameAscending: packages = packages.OrderBy(p => p.Title); break;
                    case SortByNameDescending: packages = packages.OrderByDescending(p => p.Title); break;
                    default: throw new InvalidOperationException("Invalid sort option");
                }

                return packages;
            };
        }

        IObservable<Image> GetPackageIcon(Uri iconUrl)
        {
            if (iconUrl == null) return defaultIcon;

            IObservable<Image> result;
            if (!iconCache.TryGetValue(iconUrl, out result))
            {
                WebRequest imageRequest;
                try { imageRequest = WebRequest.Create(iconUrl); }
                catch (InvalidOperationException) { return defaultIcon; }
                var iconStream = (from response in Observable.Defer(() => imageRequest.GetResponseAsync().ToObservable())
                                  from image in Observable.If(
                                      () => response.ContentType.StartsWith("image/") ||
                                            response.ContentType.StartsWith("application/octet-stream"),
                                      Observable.Using(
                                          () => response.GetResponseStream(),
                                          stream =>
                                          {
                                              try
                                              {
                                                  var image = Image.FromStream(stream);
                                                  return Observable.Return(new Bitmap(image, packageIcons.ImageSize));
                                              }
                                              catch (ArgumentException) { return defaultIcon; }
                                          }),
                                      defaultIcon)
                                  select image)
                                  .Catch<Image, WebException>(ex => defaultIcon)
                                  .Timeout(DefaultIconTimeout, defaultIcon ?? Observable.Return(DefaultIconImage))
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

        private void SetPackageViewStatus(string text, Image image = null)
        {
            if (packageView.Nodes.ContainsKey(text)) return;
            multiOperationPanel.Visible = false;
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

        private void AddPackageRange(IList<IPackage> packages, string operationName)
        {
            if (packages.Count > 0)
            {
                if (packages.Count > 1 && packagePageSelector.SelectedIndex == 0 &&
                    operationName == Resources.UpdateOperationName)
                {
                    multiOperationLabel.Text = Resources.MultipleUpdatesLabel;
                    multiOperationButton.Text = Resources.MultipleUpdatesOperationName;
                    multiOperationPanel.Visible = true;
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

        private void AddPackage(IPackage package)
        {
            var installCheck = false;
            if (selectedRepository != selectedManager.LocalRepository &&
                packageView.OperationText != Resources.UpdateOperationName)
            {
                var installedPackage = selectedManager.LocalRepository.FindPackage(package.Id);
                installCheck = installedPackage != null && installedPackage.Version >= package.Version;
            }

            var nodeTitle = !string.IsNullOrWhiteSpace(package.Title) ? package.Title : package.Id;
            var nodeText = string.Join(
                Environment.NewLine, nodeTitle,
                package.Summary ?? package.Description.Split(
                    new[] { Environment.NewLine, "\n", "\r" },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
            var node = packageView.Nodes.Add(package.Id, nodeText);
            node.Checked = installCheck;
            node.Tag = package;

            var requestIcon = GetPackageIcon(package.IconUrl);
            var iconRequest = requestIcon.ObserveOn(this).Subscribe(image =>
            {
                if (packageIcons.Images.Count == 0)
                {
                    var defaultImage = defaultIcon.Wait();
                    packageIcons.Images.Add(defaultImage);
                }
                packageIcons.Images.Add(package.Id, image);
                node.ImageKey = package.Id;
                node.SelectedImageKey = package.Id;
            });

            activeRequests.Add(iconRequest);
        }

        private void UpdatePackagePage()
        {
            ClearActiveRequests();
            SetPackageViewStatus(Resources.RetrievingInformationLabel, Resources.WaitImage);

            var packageFeed = GetPackageFeed();
            var operationName = packageView.OperationText;
            var pageIndex = packagePageSelector.SelectedIndex;
            var feedRequest = Observable.Defer(() =>
                packageFeed().AsBufferedEnumerable(PackagesPerPage * 3)
                .Where(PackageExtensions.IsListed)
                .AsCollapsed()
                .Skip(pageIndex * PackagesPerPage)
                .Take(PackagesPerPage)
                .ToObservable()
                .Catch<IPackage, WebException>(ex => Observable.Empty<IPackage>()))
                .Buffer(PackagesPerPage)
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(this)
                .Do(packages => AddPackageRange(packages, operationName))
                .Sum(packages => packages.Count)
                .Subscribe(packageCount =>
                {
                    if (packageCount == 0)
                    {
                        packagePageSelector.PageCount = pageIndex;
                        if (feedExceptionMessage != null) SetPackageViewStatus(feedExceptionMessage);
                        else if (packageView.OperationText == Resources.UpdateOperationName)
                        {
                            SetPackageViewStatus(Resources.NoUpdatesAvailableLabel);
                        }
                        else SetPackageViewStatus(Resources.NoItemsFoundLabel);
                    }
                    else if (packageCount < PackagesPerPage)
                    {
                        packagePageSelector.PageCount = pageIndex + 1;
                    }
                });

            activeRequests.Add(feedRequest);
        }

        private void UpdatePackageFeed(int selectedPage = 0)
        {
            feedExceptionMessage = null;
            SetPackageViewStatus(Resources.RetrievingInformationLabel, Resources.WaitImage);
            var packageFeed = GetPackageFeed();
            activeRequests.Add(Observable.Start(() => packageFeed().Count())
                .Catch<int, WebException>(ex =>
                {
                    feedExceptionMessage = ex.Message;
                    return Observable.Return(0);
                })
                .ObserveOn(this)
                .Subscribe(count =>
                {
                    var pageCount = count / PackagesPerPage;
                    if (count % PackagesPerPage != 0) pageCount++;
                    packagePageSelector.PageCount = pageCount;
                    packagePageSelector.SelectedIndex = selectedPage < pageCount ? selectedPage : 0;
                }));
        }

        private void RunPackageOperation(IEnumerable<IPackage> packages, bool handleDependencies)
        {
            using (var dialog = new PackageOperationDialog())
            {
                var logger = selectedManager.Logger;
                dialog.RegisterEventLogger((EventLogger)logger);

                IObservable<Unit> operation;
                if (selectedRepository == selectedManager.LocalRepository)
                {
                    operation = Observable.Start(() =>
                    {
                        foreach (var package in packages)
                        {
                            selectedManager.UninstallPackage(package, false, handleDependencies);
                        }
                    });
                    dialog.Text = Resources.UninstallOperationLabel;
                }
                else
                {
                    var allowPrereleaseVersions = AllowPrereleaseVersions;
                    var update = packageView.OperationText == Resources.UpdateOperationName;
                    dialog.Text = update ? Resources.UpdateOperationLabel : Resources.InstallOperationLabel;

                    operation = Observable.Start(() =>
                    {
                        foreach (var package in packages)
                        {
                            if (update || selectedManager.LocalRepository.FindPackage(package.Id) != null)
                            {
                                selectedManager.UpdatePackage(package, handleDependencies, allowPrereleaseVersions);
                            }
                            else
                            {
                                selectedManager.InstallPackage(package, !handleDependencies, allowPrereleaseVersions);
                            }
                        }
                    });
                }

                operationDialog = dialog;
                try
                {
                    operation.ObserveOn(this).Subscribe(
                        xs => { },
                        ex => logger.Log(MessageLevel.Error, ex.Message),
                        () => dialog.Complete());
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        UpdatePackageFeed(packagePageSelector.SelectedIndex);
                    }
                }
                finally { operationDialog = null; }
            }
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            packageView.BeginUpdate();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            packageView.EndUpdate();
            packageView.Refresh();
            base.OnResizeEnd(e);
        }

        private void PackageManagerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E && e.Modifiers == Keys.Control && !searchComboBox.Focused)
            {
                searchComboBox.Select();
            }
        }

        private void packageView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            packageDetails.SetPackage((IPackage)e.Node.Tag);
        }

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                UpdatePackageFeed();
            }
        }

        private void packagePageSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePackagePage();
        }

        private void multiOperationButton_Click(object sender, EventArgs e)
        {
            if (packageView.OperationText == Resources.UpdateOperationName)
            {
                var packageFeed = GetPackageFeed();
                var packages = packageFeed()
                    .AsEnumerable()
                    .Where(PackageExtensions.IsListed)
                    .AsCollapsed();
                RunPackageOperation(packages, true);
            }
        }

        private void packageView_OperationClick(object sender, TreeViewEventArgs e)
        {
            bool handleDependencies = true;
            var package = (IPackage)e.Node.Tag;
            if (package != null)
            {
                if (selectedRepository == selectedManager.LocalRepository)
                {
                    var dependencies = (from dependency in package.GetCompatiblePackageDependencies(null)
                                        let dependencyPackage = selectedRepository.ResolveDependency(dependency, true, true)
                                        where dependencyPackage != null
                                        select dependencyPackage)
                                        .ToArray();
                    if (dependencies.Length > 0)
                    {
                        var dependencyNotice = new StringBuilder();
                        dependencyNotice.AppendLine(string.Format(Resources.PackageDependencyNotice, package));
                        foreach (var dependency in dependencies)
                        {
                            dependencyNotice.AppendLine(dependency.ToString());
                        }

                        var result = MessageBox.Show(this, dependencyNotice.ToString(), Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Cancel) return;
                        if (result == DialogResult.No) handleDependencies = false;
                    }
                }

                RunPackageOperation(new[] { package }, handleDependencies);
            }
        }

        private void repositoriesView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            collapsingNode = e.Node;
        }

        private void repositoriesView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            collapsingNode = null;
            if (repositoriesView.SelectedNode == e.Node && selectingNode == null)
            {
                repositoriesView.SelectedNode = null;
            }
        }

        private void repositoriesView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (repositoriesView.SelectedNode != e.Node)
            {
                repositoriesView.SelectedNode = e.Node;
            }
        }

        private void repositoriesView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectingNode = null;
            selectedManager = e.Node.Tag as PackageManager;
            if (selectedManager == null) return;
            if (e.Node == installedPackagesNode || e.Node.Parent == installedPackagesNode)
            {
                releaseFilterComboBox.Visible = false;
                packageView.OperationText = Resources.UninstallOperationName;
                selectedRepository = selectedManager.LocalRepository;
            }
            else
            {
                releaseFilterComboBox.Visible = true;
                selectedRepository = selectedManager.SourceRepository;
                if (e.Node == updatesNode || e.Node.Parent == updatesNode)
                {
                    packageView.OperationText = Resources.UpdateOperationName;
                }
                else packageView.OperationText = Resources.InstallOperationName;
            }

            searchComboBox.Text = string.Empty;
            UpdatePackageFeed();
        }

        private void repositoriesView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            selectingNode = node;
            if (node != collapsingNode && node.Parent == null)
            {
                e.Cancel = true;
                var selectedNode = repositoriesView.SelectedNode;
                if (selectedNode != null && selectedNode.Parent != null) selectedNode = selectedNode.Parent;
                if (selectedNode != null) selectedNode.Collapse();

                node.Expand();
                var selectedChild = e.Node.Nodes[0];
                repositoriesView.SelectedNode = selectedChild;
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            Hide();
            using (var dialog = new PackageSourceConfigurationDialog(packageSourceProvider))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedManager = null;
                    selectedRepository = null;
                    feedExceptionMessage = null;
                    packageManagers = CreatePackageManagers();
                    InitializeRepositoryViewNodes();
                    SelectDefaultNode();
                }
            }
            Show();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region IPackageManager Members

        public IFileSystem FileSystem
        {
            get { return selectedManager != null ? selectedManager.FileSystem : null; }
            set { if (selectedManager != null)selectedManager.FileSystem = value; }
        }

        public void InstallPackage(string packageId, SemanticVersion version, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.InstallPackage(packageId, version, ignoreDependencies, allowPrereleaseVersions);
            }
        }

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions, bool ignoreWalkInfo)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions, ignoreWalkInfo);
            }
        }

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions);
            }
        }

        public IPackageRepository LocalRepository
        {
            get { return selectedManager != null ? selectedManager.LocalRepository : null; }
        }

        public ILogger Logger
        {
            get { return selectedManager != null ? selectedManager.Logger : null; }
            set { if (selectedManager != null) selectedManager.Logger = value; }
        }

        public event EventHandler<PackageOperationEventArgs> PackageInstalled;

        public event EventHandler<PackageOperationEventArgs> PackageInstalling;

        public event EventHandler<PackageOperationEventArgs> PackageUninstalled;

        public event EventHandler<PackageOperationEventArgs> PackageUninstalling;

        private void OnPackageInstalled(PackageOperationEventArgs e)
        {
            var handler = PackageInstalled;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageInstalling(PackageOperationEventArgs e)
        {
            var handler = PackageInstalling;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageUninstalled(PackageOperationEventArgs e)
        {
            var handler = PackageUninstalled;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageUninstalling(PackageOperationEventArgs e)
        {
            var handler = PackageUninstalling;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public IPackagePathResolver PathResolver
        {
            get { return selectedManager != null ? selectedManager.PathResolver : null; }
        }

        public IPackageRepository SourceRepository
        {
            get { return selectedManager != null ? selectedManager.SourceRepository : null; }
        }

        public void UninstallPackage(string packageId, SemanticVersion version, bool forceRemove, bool removeDependencies)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.UninstallPackage(packageId, version, forceRemove, removeDependencies);
            }
        }

        public void UninstallPackage(IPackage package, bool forceRemove, bool removeDependencies)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.UninstallPackage(package, forceRemove, removeDependencies);
            }
        }

        public void UpdatePackage(string packageId, IVersionSpec versionSpec, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.UpdatePackage(packageId, versionSpec, updateDependencies, allowPrereleaseVersions);
            }
        }

        public void UpdatePackage(string packageId, SemanticVersion version, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.UpdatePackage(packageId, version, updateDependencies, allowPrereleaseVersions);
            }
        }

        public void UpdatePackage(IPackage newPackage, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = selectedManager;
            if (manager != null)
            {
                manager.UpdatePackage(newPackage, updateDependencies, allowPrereleaseVersions);
            }
        }

        #endregion

        public DependencyVersion DependencyVersion
        {
            get { return selectedManager != null ? selectedManager.DependencyVersion : default(DependencyVersion); }
            set
            {
                var manager = selectedManager;
                if (manager != null)
                {
                    manager.DependencyVersion = value;
                }
            }
        }

        public bool WhatIf
        {
            get { return selectedManager != null ? selectedManager.WhatIf : default(bool); }
            set
            {
                var manager = selectedManager;
                if (manager != null)
                {
                    manager.WhatIf = value;
                }
            }
        }
    }
}
