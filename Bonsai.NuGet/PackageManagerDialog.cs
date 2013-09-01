using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
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
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");

        bool loaded;
        readonly IPackageRepository packageRepository;
        readonly IPackageManager packageManager;

        public PackageManagerDialog(IPackageRepository repository, IPackageManager manager)
        {
            packageRepository = repository;
            packageManager = manager;
            packageManager.Logger = new EventLogger();
            InitializeComponent();
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
            get { return releaseFilterComboBox.SelectedIndex == 1; }
        }

        IQueryable<IPackage> GetPackageFeed()
        {
            var packages = packageRepository
                .Search(searchComboBox.Text, AllowPrereleaseVersions)
                .Where(p => p.IsLatestVersion);
            switch ((string)sortComboBox.SelectedItem)
            {
                case SortByRelevance: break;
                case SortByMostDownloads: packages = packages.OrderByDescending(p => p.DownloadCount); break;
                case SortByPublishedDate: packages = packages.OrderByDescending(p => p.Published); break;
                case SortByNameAscending: packages = packages.OrderBy(p => p.Title); break;
                case SortByNameDescending: packages = packages.OrderByDescending(p => p.Title); break;
                default: throw new InvalidOperationException("Invalid sort option");
            }

            return packages;
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
                var packages = GetPackageFeed()
                    .Skip(packagePageSelector.SelectedIndex * PackagesPerPage)
                    .Take(PackagesPerPage);

                packages.ToObservable()
                        .SubscribeOn(NewThreadScheduler.Default)
                        .ObserveOn(this)
                        .Subscribe(package => AddPackage(package));
            }
            else packageView.Nodes.Add("No items found.");
        }

        private void UpdatePackageFeed()
        {
            var packages = GetPackageFeed();
            Observable.Start(() => packages.Count())
                .ObserveOn(this)
                .Subscribe(count => packagePageSelector.PageCount = count / PackagesPerPage);
        }

        private bool IsPackageLicenseAcceptanceRequired(IPackage package)
        {
            return package.RequireLicenseAcceptance;
        }

        private IEnumerable<IPackage> GetPackageLicenseRequirements(IPackage package, bool allowPrereleaseVersions)
        {
            if (IsPackageLicenseAcceptanceRequired(package))
            {
                yield return package;
            }

            foreach (var dependency in package.GetCompatiblePackageDependencies(new FrameworkName("net40")))
            {
                var dependencyPackage = packageRepository.FindPackage(dependency.Id, dependency.VersionSpec, allowPrereleaseVersions, false);
                foreach (var requirement in GetPackageLicenseRequirements(dependencyPackage, allowPrereleaseVersions))
                {
                    yield return requirement;
                }
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

        private void packageView_InstallClick(object sender, TreeViewEventArgs e)
        {
            var package = (IPackage)e.Node.Tag;
            if (package != null)
            {
                var dialog = new PackageInstallDialog();
                dialog.RegisterEventLogger((EventLogger)packageManager.Logger);
                var allowPrereleaseVersions = AllowPrereleaseVersions;
                var installation = Observable.Start(() => packageManager.InstallPackage(package, false, allowPrereleaseVersions, false));
                installation.ObserveOn(this).Subscribe(xs => dialog.Close());
                dialog.ShowDialog();
            }
        }
    }
}
