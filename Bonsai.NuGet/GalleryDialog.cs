using Bonsai.Design;
using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
    public partial class GalleryDialog : Form
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
        string targetPath;
        IPackage targetPackage;
        readonly string packageManagerPath;
        readonly IPackageSourceProvider packageSourceProvider;
        Dictionary<string, PackageManager> packageManagers;
        PackageManagerProxy packageManagerProxy;
        IPackageRepository selectedRepository;
        string feedExceptionMessage;
        List<IDisposable> activeRequests;
        IDisposable searchSubscription;
        Form operationDialog;

        public GalleryDialog(string path)
        {
            InitializeComponent();
            packageManagerPath = path;
            iconCache = new ConcurrentDictionary<Uri, IObservable<Image>>();
            defaultIcon = GetPackageIcon(PackageDefaultIconUrl);

            activeRequests = new List<IDisposable>();
            packageManagerProxy = new PackageManagerProxy();
            packageManagerProxy.PackageInstalling += packageManagerProxy_PackageInstalling;
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(null, null, machineWideSettings);
            packageSourceProvider = new PackageSourceProvider(settings);
            packageManagers = CreatePackageManagers();
            searchComboBox.CueBanner = Resources.SearchOnlineCueBanner;

            sortComboBox.Items.Add(SortByMostDownloads);
            sortComboBox.Items.Add(SortByPublishedDate);
            sortComboBox.Items.Add(SortByNameAscending);
            sortComboBox.Items.Add(SortByNameDescending);
            sortComboBox.SelectedIndex = 0;
            releaseFilterComboBox.SelectedIndex = 0;
        }

        public string InstallPath { get; set; }

        public IPackageManager PackageManager
        {
            get { return packageManagerProxy; }
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
            packageManager.RequiringLicenseAcceptance += packageManager_RequiringLicenseAcceptance;
            packageManager.Logger = logger;
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
            var aggregateRepository = packageSourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, true);
            var aggregatePackageManager = CreatePackageManager(aggregateRepository, logger);
            managers.Add(Resources.AllNodeName, aggregatePackageManager);
            packageManagerProxy.PackageManager = aggregatePackageManager;

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
            UpdateSelectedRepository();
            searchComboBox.Select();
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

        Func<IQueryable<IPackage>> GetPackageFeed()
        {
            var searchTerm = searchComboBox.Text;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            var sortMode = (string)sortComboBox.SelectedItem;
            return () =>
            {
                if (selectedRepository == null || packageManagerProxy.PackageManager == null)
                {
                    return Enumerable.Empty<IPackage>().AsQueryable();
                }

                IQueryable<IPackage> packages;
                try { packages = selectedRepository.GetPackages().Find(searchTerm); }
                catch (WebException e) { return Observable.Throw<IPackage>(e).ToEnumerable().AsQueryable(); }
                if (allowPrereleaseVersions) packages = packages.Where(p => p.IsAbsoluteLatestVersion);
                else packages = packages.Where(p => p.IsLatestVersion);

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

        private void AddPackageRange(IList<IPackage> packages)
        {
            if (packages.Count > 0)
            {
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
            if (selectedRepository != packageManagerProxy.LocalRepository &&
                packageView.OperationText != Resources.UpdateOperationName)
            {
                var installedPackage = packageManagerProxy.LocalRepository.FindPackage(package.Id);
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
                .Do(packages => AddPackageRange(packages))
                .Sum(packages => packages.Count)
                .Subscribe(packageCount =>
                {
                    if (packageCount == 0)
                    {
                        packagePageSelector.PageCount = pageIndex;
                        if (feedExceptionMessage != null) SetPackageViewStatus(feedExceptionMessage);
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

        private void RunPackageOperation(IPackage package, string path)
        {
            using (var dialog = new PackageOperationDialog())
            {
                var logger = packageManagerProxy.Logger;
                dialog.RegisterEventLogger((EventLogger)logger);

                var allowPrereleaseVersions = AllowPrereleaseVersions;
                dialog.Text = Resources.InstallOperationLabel;

                var operation = Observable.Start(() =>
                {
                    targetPath = path;
                    targetPackage = package;
                    packageManagerProxy.InstallPackage(package, false, allowPrereleaseVersions);
                });

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

        private void packageView_OperationClick(object sender, TreeViewEventArgs e)
        {
            var package = (IPackage)e.Node.Tag;
            if (package != null)
            {
                saveFileDialog.FileName = package.Id;
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    RunPackageOperation(package, saveFileDialog.FileName);
                    if (DialogResult == DialogResult.OK)
                    {
                        Close();
                    }
                }
            }
        }

        private void UpdateSelectedRepository()
        {
            if (packageManagerProxy.PackageManager == null) return;
            selectedRepository = packageManagerProxy.SourceRepository;
            packageView.OperationText = Resources.OpenOperationName;
            searchComboBox.Text = string.Empty;
            UpdatePackageFeed();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            Hide();
            using (var dialog = new PackageSourceConfigurationDialog(packageSourceProvider))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedRepository = null;
                    feedExceptionMessage = null;
                    packageManagerProxy.PackageManager = null;
                    packageManagers = CreatePackageManagers();
                    UpdateSelectedRepository();
                }
            }
            Show();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void packageManagerProxy_PackageInstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            if (package == targetPackage)
            {
                var workflowPath = package.Id + Constants.BonsaiExtension;
                if (!package.GetContentFiles().Any(file => file.EffectivePath == workflowPath))
                {
                    var message = string.Format(Resources.MissingWorkflowEntryPoint, workflowPath);
                    throw new InvalidOperationException(message);
                }

                var targetFileSystem = new PhysicalFileSystem(targetPath);
                foreach (var file in package.GetContentFiles())
                {
                    using (var stream = file.GetStream())
                    {
                        targetFileSystem.AddFile(file.EffectivePath, stream);
                    }
                }

                var manifest = Manifest.Create(package);
                var metadata = Manifest.Create(manifest.Metadata);
                var metadataPath = package.Id + global::NuGet.Constants.ManifestExtension;
                using (var stream = targetFileSystem.CreateFile(metadataPath))
                {
                    metadata.Save(stream);
                }

                InstallPath = targetFileSystem.GetFullPath(workflowPath);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
