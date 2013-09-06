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
    public partial class PackageManagerDialog : Form
    {
        const int PackagesPerPage = 10;
        const string SortByMostDownloads = "Most Downloads";
        const string SortByPublishedDate = "Published Date";
        const string SortByNameAscending = "Name: Ascending";
        const string SortByNameDescending = "Name: Descending";
        const string SortByRelevance = "Relevance";
        const string PackageSourceSectionName = "packageSources";
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");

        bool loaded;
        readonly ISettings settings;
        readonly string packageManagerPath;
        readonly Dictionary<string, PackageManager> packageManagers;
        IPackageManager selectedManager;
        IPackageRepository selectedRepository;

        TreeNode installedPackagesNode;
        TreeNode onlineNode;
        TreeNode updatesNode;

        public PackageManagerDialog(string path)
        {
            packageManagerPath = path;
            settings = Settings.LoadDefaultSettings(null, null, null);
            packageManagers = CreatePackageManagers();
            InitializeComponent();
            InitializeRepositoryViewNodes();
        }

        private Dictionary<string, PackageManager> CreatePackageManagers()
        {
            var packageSources = (from settingValue in settings.GetSettingValues(PackageSourceSectionName, false)
                                  select new
                                  {
                                      name = settingValue.Key,
                                      source = PackageRepositoryFactory.Default.CreateRepository(settingValue.Value)
                                  }).ToList();

            var allPackageSource = new AggregateRepository(packageSources.Select(xs => xs.source));
            var aggregateSources =
                Enumerable.Repeat(new { name = Resources.AllNodeName, source = (IPackageRepository)allPackageSource }, 1)
                .Concat(packageSources);

            var managers = new Dictionary<string, PackageManager>();
            foreach (var packageSource in aggregateSources)
            {
                var packageManager = new PackageManager(packageSource.source, packageManagerPath);
                packageManager.Logger = new EventLogger();
                managers.Add(packageSource.name, packageManager);
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
            var imageRequest = WebRequest.Create(iconUrl == null ? PackageDefaultIconUrl : iconUrl);
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
                        GetPackageIcon(null))
                    select image)
                    .Catch<Image, WebException>(ex => GetPackageIcon(null));
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
                    .ToObservable())
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOn(this)
                    .Subscribe(package => AddPackage(package));
            }
            else packageView.Nodes.Add("No items found.");
        }

        private void UpdatePackageFeed()
        {
            var packageFeed = GetPackageFeed();
            Observable.Start(() => packageFeed().Count())
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
            var package = (IPackage)e.Node.Tag;
            if (package != null)
            {
                using (var dialog = new PackageOperationDialog())
                {
                    var logger = selectedManager.Logger;
                    dialog.RegisterEventLogger((EventLogger)logger, selectedManager);

                    IObservable<Unit> operation;
                    if (selectedRepository == selectedManager.LocalRepository)
                    {
                        operation = Observable.Start(() => selectedManager.UninstallPackage(package, false, false));
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
                }
            }
        }

        private void repositoriesView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedManager = e.Node.Tag as IPackageManager;
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
            using (var dialog = new PackageSourceConfigurationDialog(packageSourceProvider))
            {
                dialog.ShowDialog(this);
            }
        }
    }
}
