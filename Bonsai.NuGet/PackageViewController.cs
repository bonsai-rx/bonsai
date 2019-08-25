using Bonsai.Design;
using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    class PackageViewController
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
        static readonly IEnumerable<string> TagProperties = Enumerable.Repeat("Tags", 1);
        readonly ConcurrentDictionary<Uri, IObservable<Image>> iconCache;
        readonly IObservable<Image> defaultIcon;

        bool loaded;
        readonly string packageManagerPath;
        readonly IEnumerable<string> tagSearchTerms;
        readonly IPackageSourceProvider packageSourceProvider;
        Dictionary<string, PackageManager> packageManagers;
        PackageManagerProxy packageManagerProxy;
        IPackageRepository selectedRepository;
        string feedExceptionMessage;
        List<IDisposable> activeRequests;
        IDisposable searchSubscription;
        Form operationDialog;

        Control control;
        PackageView packageView;
        PackageDetails packageDetails;
        PackagePageSelector packagePageSelector;
        ImageList packageIcons;
        CueBannerComboBox searchComboBox;
        ComboBox sortComboBox;
        ComboBox releaseFilterComboBox;
        Func<bool> getUpdateFeed;
        Action<bool> setMultiOperationVisible;

        public PackageViewController(
            string path,
            Control owner,
            PackageView view,
            PackageDetails details,
            PackagePageSelector pageSelector,
            PackageManagerProxy managerProxy,
            ImageList icons,
            CueBannerComboBox search,
            ComboBox sort,
            ComboBox releaseFilter,
            Func<bool> updateFeed,
            Action<bool> multiOperationVisible,
            IEnumerable<string> tagConstraints)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            if (view == null) throw new ArgumentNullException("view");
            if (details == null) throw new ArgumentNullException("details");
            if (pageSelector == null) throw new ArgumentNullException("pageSelector");
            if (managerProxy == null) throw new ArgumentNullException("managerProxy");
            if (icons == null) throw new ArgumentNullException("icons");
            if (search == null) throw new ArgumentNullException("search");
            if (sort == null) throw new ArgumentNullException("sort");
            if (releaseFilter == null) throw new ArgumentNullException("releaseFilter");
            if (updateFeed == null) throw new ArgumentNullException("updateFeed");
            if (multiOperationVisible == null) throw new ArgumentNullException("multiOperationVisible");
            if (tagConstraints == null) throw new ArgumentNullException("tagConstraints");

            control = owner;
            packageView = view;
            packageDetails = details;
            packagePageSelector = pageSelector;
            packageManagerProxy = managerProxy;
            packageIcons = icons;
            searchComboBox = search;
            sortComboBox = sort;
            releaseFilterComboBox = releaseFilter;
            getUpdateFeed = updateFeed;
            setMultiOperationVisible = multiOperationVisible;
            tagSearchTerms = tagConstraints;
            control.KeyDown += control_KeyDown;
            packageView.AfterSelect += packageView_AfterSelect;
            sortComboBox.SelectedIndexChanged += filterComboBox_SelectedIndexChanged;
            releaseFilterComboBox.SelectedIndexChanged += filterComboBox_SelectedIndexChanged;
            packagePageSelector.SelectedIndexChanged += packagePageSelector_SelectedIndexChanged;

            packageManagerPath = path;
            iconCache = new ConcurrentDictionary<Uri, IObservable<Image>>();
            defaultIcon = GetPackageIcon(PackageDefaultIconUrl);

            activeRequests = new List<IDisposable>();
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

        public IPackageRepository SelectedRepository
        {
            get { return selectedRepository; }
            set { selectedRepository = value; }
        }

        public Dictionary<string, PackageManager> PackageManagers
        {
            get { return packageManagers; }
        }

        public void ClearActiveRequests()
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

        private Dictionary<string, PackageManager> CreatePackageManagers()
        {
            var logger = new EventLogger();
            var managers = new Dictionary<string, PackageManager>();
            var aggregateRepository = packageSourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, true);
            var aggregatePackageManager = CreatePackageManager(aggregateRepository, logger);
            managers.Add(Resources.AllNodeName, aggregatePackageManager);
            packageManagerProxy.PackageManager = aggregatePackageManager;
            packageManagerProxy.SourceRepository = aggregatePackageManager.SourceRepository;

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

        public void OnLoad(EventArgs e)
        {
            searchSubscription = Observable.FromEventPattern<EventArgs>(
                handler => searchComboBox.TextChanged += new EventHandler(handler),
                handler => searchComboBox.TextChanged -= new EventHandler(handler))
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(control)
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
        }

        public void OnHandleDestroyed(EventArgs e)
        {
            ClearActiveRequests();
            searchSubscription.Dispose();
        }

        bool AllowPrereleaseVersions
        {
            get
            {
                return releaseFilterComboBox.SelectedIndex == 1 ||
                    selectedRepository == packageManagers[Resources.AllNodeName].LocalRepository;
            }
        }

        public Func<IQueryable<IPackage>> GetPackageFeed()
        {
            var searchTerm = searchComboBox.Text;
            var allowPrereleaseVersions = AllowPrereleaseVersions;
            var sortMode = (string)sortComboBox.SelectedItem;
            var updateFeed = getUpdateFeed();
            return () =>
            {
                if (selectedRepository == null || packageManagerProxy.PackageManager == null)
                {
                    return Enumerable.Empty<IPackage>().AsQueryable();
                }

                IQueryable<IPackage> packages;
                if (updateFeed)
                {
                    var localPackages = packageManagerProxy.LocalRepository.GetPackages();
                    try { packages = selectedRepository.GetUpdates(localPackages, allowPrereleaseVersions, false).AsQueryable(); }
                    catch (AggregateException e) { return Observable.Throw<IPackage>(e.InnerException).ToEnumerable().AsQueryable(); }
                    catch (WebException e) { return Observable.Throw<IPackage>(e).ToEnumerable().AsQueryable(); }
                }
                else
                {
                    try { packages = selectedRepository.GetPackages().Find(searchTerm).WithTags(tagSearchTerms); }
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

        private void AddPackageRange(IList<IPackage> packages)
        {
            if (packages.Count > 0)
            {
                if (packages.Count > 1 && packagePageSelector.SelectedIndex == 0 &&
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
            var iconRequest = requestIcon.ObserveOn(control).Subscribe(image =>
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
            var feedRequest = Observable.If(
                () => packagePageSelector.PageCount == 0,
                Observable.Empty<IPackage>(),
                Observable.Defer(() =>
                packageFeed().AsBufferedEnumerable(PackagesPerPage * 3)
                .Where(PackageExtensions.IsListed)
                .AsCollapsed()
                .Skip(pageIndex * PackagesPerPage)
                .Take(PackagesPerPage)
                .ToObservable()
                .Catch<IPackage, WebException>(ex => Observable.Empty<IPackage>())))
                .Buffer(PackagesPerPage)
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(control)
                .Do(packages => AddPackageRange(packages))
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

        public void UpdatePackageFeed(int selectedPage = 0)
        {
            feedExceptionMessage = null;
            SetPackageViewStatus(Resources.RetrievingInformationLabel, Resources.WaitImage);
            var packageFeed = GetPackageFeed();
            activeRequests.Add(Observable.Start(() => packageFeed().Count())
                .Catch<int, InvalidOperationException>(ex =>
                {
                    feedExceptionMessage = ex.Message;
                    return Observable.Return(0);
                })
                .ObserveOn(control)
                .Subscribe(count =>
                {
                    var pageCount = count / PackagesPerPage;
                    if (count % PackagesPerPage != 0) pageCount++;
                    packagePageSelector.PageCount = pageCount;
                    packagePageSelector.SelectedIndex = selectedPage < pageCount ? selectedPage : 0;
                }));
        }

        public void RunPackageOperation(IEnumerable<IPackage> packages, bool handleDependencies)
        {
            using (var dialog = new PackageOperationDialog())
            {
                var logger = packageManagerProxy.Logger;
                dialog.RegisterEventLogger((EventLogger)logger);

                IObservable<Unit> operation;
                if (selectedRepository == packageManagerProxy.LocalRepository)
                {
                    operation = Observable.Start(() =>
                    {
                        foreach (var package in packages)
                        {
                            packageManagerProxy.UninstallPackage(package, false, handleDependencies);
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
                            packageManagerProxy.InstallPackage(package, !handleDependencies, allowPrereleaseVersions);
                        }
                    });
                }

                operationDialog = dialog;
                try
                {
                    operation.ObserveOn(control).Subscribe(
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

        public DialogResult ShowPackageSourceConfigurationDialog()
        {
            using (var dialog = new PackageSourceConfigurationDialog(packageSourceProvider))
            {
                var result = dialog.ShowDialog(control);
                if (result == DialogResult.OK)
                {
                    selectedRepository = null;
                    feedExceptionMessage = null;
                    packageManagerProxy.PackageManager = null;
                    packageManagers = CreatePackageManagers();
                }
                return result;
            }
        }
    }
}
