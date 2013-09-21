using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
        readonly IObservable<Image> defaultIcon;

        bool loaded;
        readonly string packageManagerPath;
        readonly PackageSourceProvider packageSourceProvider;
        Dictionary<string, IPackageManager> packageManagers;
        IPackageManager selectedManager;
        IPackageRepository selectedRepository;
        string feedExceptionMessage;

        TreeNode installedPackagesNode;
        TreeNode onlineNode;
        TreeNode updatesNode;

        public PackageManagerDialog(string path)
        {
            packageManagerPath = path;
            var defaultIconRequest = GetPackageIcon(PackageDefaultIconUrl).PublishLast();
            defaultIcon = defaultIconRequest;
            defaultIconRequest.Connect();

            var settings = Settings.LoadDefaultSettings(null, null, null);
            packageSourceProvider = new PackageSourceProvider(settings);
            packageManagers = CreatePackageManagers();
            InitializeComponent();
            InitializeRepositoryViewNodes();
        }

        IPackageManager CreatePackageManager(IPackageRepository sourceRepository, EventLogger logger)
        {
            var packageManager = new PackageManager(sourceRepository, packageManagerPath);
            packageManager.Logger = logger;
            packageManager.PackageInstalled += (sender, e) => OnPackageInstalled(e);
            packageManager.PackageInstalling += (sender, e) => OnPackageInstalling(e);
            packageManager.PackageUninstalled += (sender, e) => OnPackageUninstalled(e);
            packageManager.PackageUninstalling += (sender, e) => OnPackageUninstalling(e);
            return packageManager;
        }

        private Dictionary<string, IPackageManager> CreatePackageManagers()
        {
            var logger = new EventLogger();
            var managers = new Dictionary<string, IPackageManager>();
            var aggregateRepository = packageSourceProvider.GetAggregate(PackageRepositoryFactory.Default);
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

            updatesNode = repositoriesView.Nodes.Add(Resources.UpdatesNodeName);
            InitializeRepositoryViewNode(updatesNode);
        }

        protected override void OnLoad(EventArgs e)
        {
            sortComboBox.Items.Add(SortByMostDownloads);
            sortComboBox.Items.Add(SortByPublishedDate);
            sortComboBox.Items.Add(SortByNameAscending);
            sortComboBox.Items.Add(SortByNameDescending);
            releaseFilterComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndex = 0;
            UpdatePackageFeed();
            Observable.FromEventPattern<EventArgs>(
                handler => searchComboBox.TextChanged += new EventHandler(handler),
                handler => searchComboBox.TextChanged -= new EventHandler(handler))
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(this)
                .Subscribe(evt =>
                {
                    if (!string.IsNullOrWhiteSpace(searchComboBox.Text))
                    {
                        sortComboBox.Items.Insert(0, SortByRelevance);
                    }
                    else sortComboBox.Items.Remove(SortByRelevance);
                    sortComboBox.SelectedIndex = 0;
                    UpdatePackageFeed();
                });

            loaded = true;
            base.OnLoad(e);
        }

        bool AllowPrereleaseVersions
        {
            get
            {
                return releaseFilterComboBox.SelectedIndex == 1 ||
                    selectedRepository == packageManagers[Resources.AllNodeName].LocalRepository;
            }
        }

        Func<IQueryable<IPackage>> GetPackageFeed()
        {
            var searchTerm = searchComboBox.Text;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            var sortMode = (string)sortComboBox.SelectedItem;
            var updateFeed = updatesNode.IsExpanded;
            return () =>
            {
                if (selectedRepository == null)
                {
                    return Enumerable.Empty<IPackage>().AsQueryable();
                }

                IQueryable<IPackage> packages;
                if (updateFeed)
                {
                    packages = selectedRepository.GetUpdates(
                        selectedManager.LocalRepository.GetPackages(),
                        allowPrereleaseVersions,
                        false).AsQueryable();
                }
                else
                {
                    packages = selectedRepository
                        .Search(searchTerm, allowPrereleaseVersions)
                        .Where(p => p.IsAbsoluteLatestVersion);
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

            var imageRequest = WebRequest.Create(iconUrl);
            var requestAsync = Observable.FromAsyncPattern(
                (callback, state) => imageRequest.BeginGetResponse(callback, state),
                asyncResult => imageRequest.EndGetResponse(asyncResult));

            return (from response in Observable.Defer(() => requestAsync())
                    from image in Observable.If(
                        () => iconUrl == null ||
                              response.ContentType.StartsWith("image/") ||
                              response.ContentType.StartsWith("application/octet-stream"),
                        Observable.Defer(() =>
                            Observable.Return(new Bitmap(Image.FromStream(response.GetResponseStream()), packageIcons.ImageSize))),
                        defaultIcon)
                    select image)
                    .Catch<Image, WebException>(ex => defaultIcon);
        }

        private void AddPackage(IPackage package)
        {
            if (!packageView.Nodes.ContainsKey(package.Id))
            {
                var nodeTitle = !string.IsNullOrWhiteSpace(package.Title) ? package.Title : package.Id;
                var nodeText = string.Join(
                    Environment.NewLine, nodeTitle,
                    package.Summary ?? package.Description.Split(
                        new[] { Environment.NewLine, "\n", "\r" },
                        StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
                var node = packageView.Nodes.Add(package.Id, nodeText);
                node.Tag = package;

                var requestIcon = GetPackageIcon(package.IconUrl);
                requestIcon.ObserveOn(this).Subscribe(image =>
                {
                    packageIcons.Images.Add(package.Id, image);
                    node.ImageKey = package.Id;
                    node.SelectedImageKey = package.Id;
                });
            }
        }

        private void UpdatePackagePage()
        {
            packageView.Nodes.Clear();
            packageIcons.Images.Clear();

            if (packagePageSelector.PageCount > 0)
            {
                var packageFeed = GetPackageFeed();
                var pageIndex = packagePageSelector.SelectedIndex;
                Observable.Defer(() =>
                    packageFeed()
                    .Skip(pageIndex * PackagesPerPage)
                    .Take(PackagesPerPage)
                    .ToObservable()
                    .Catch<IPackage, WebException>(ex => Observable.Empty<IPackage>()))
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOn(this)
                    .Subscribe(package => AddPackage(package));
            }
            else
            {
                packageView.Nodes.Add(feedExceptionMessage ?? Resources.NoItemsFoundLabel);
                packageDetails.SetPackage(null);
            }
        }

        private void UpdatePackageFeed()
        {
            feedExceptionMessage = null;
            var packageFeed = GetPackageFeed();
            Observable.Start(() => packageFeed().Count())
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
                });
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

        private void packageView_OperationClick(object sender, TreeViewEventArgs e)
        {
            bool removeDependencies = false;
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
                        if (result == DialogResult.Yes) removeDependencies = true;
                    }
                }

                using (var dialog = new PackageOperationDialog())
                {
                    var logger = selectedManager.Logger;
                    dialog.RegisterEventLogger((EventLogger)logger);

                    IObservable<Unit> operation;
                    if (selectedRepository == selectedManager.LocalRepository)
                    {
                        operation = Observable.Start(() => selectedManager.UninstallPackage(package, false, removeDependencies));
                        dialog.Text = Resources.UninstallOperationLabel;
                    }
                    else
                    {
                        var allowPrereleaseVersions = AllowPrereleaseVersions;
                        operation = Observable.Start(() => selectedManager.InstallPackage(package, false, allowPrereleaseVersions, false));
                        dialog.Text = Resources.InstallOperationLabel;
                    }

                    operation.ObserveOn(this).Subscribe(
                        xs => { },
                        ex => logger.Log(MessageLevel.Error, ex.Message),
                        () => dialog.Close());
                    dialog.ShowDialog();
                    UpdatePackageFeed();
                }
            }
        }

        private void repositoriesView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedManager = e.Node.Tag as IPackageManager;
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
                packageView.OperationText = Resources.InstallOperationName;
                selectedRepository = selectedManager.SourceRepository;
            }

            searchComboBox.Text = string.Empty;
            UpdatePackageFeed();
        }

        private void repositoriesView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Parent == null && !node.IsExpanded)
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
                    UpdatePackageFeed();
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
            set { if (selectedManager != null)selectedManager.Logger = value; }
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
    }
}
