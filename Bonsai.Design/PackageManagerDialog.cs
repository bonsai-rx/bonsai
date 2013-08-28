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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public partial class PackageManagerDialog : Form
    {
        static readonly Uri PackageDefaultIconUrl = new Uri("https://www.nuget.org/Content/Images/packageDefaultIcon.png");
        readonly IPackageRepository packageRepository;
        readonly IPackageManager packageManager;

        public PackageManagerDialog(IPackageRepository repository, IPackageManager manager)
        {
            packageRepository = repository;
            packageManager = manager;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            releaseFilterComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndex = 0;
            base.OnLoad(e);
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
                        Observable.Defer(() => Observable.Return(Image.FromStream(response.GetResponseStream()))),
                        GetPackageIcon(null))
                    select image)
                    .Catch<Image, WebException>(ex => GetPackageIcon(null));
        }

        private void AddPackage(IPackage package)
        {
            if (!packageView.Nodes.ContainsKey(package.Id))
            {
                var nodeTitle = !string.IsNullOrWhiteSpace(package.Title) ? package.Title : package.Id;
                var nodeText = string.Join(Environment.NewLine, nodeTitle, package.Description);
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

        private void packageView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var color = (e.State & TreeNodeStates.Focused) != 0 ? SystemColors.HighlightText : SystemColors.WindowText;
            var bounds = e.Bounds;
            bounds.Width = packageView.Width - bounds.X;
            var bold = new Font(packageView.Font, FontStyle.Bold);

            var lines = e.Node.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            TextRenderer.DrawText(e.Graphics, lines[0], bold, bounds, color, TextFormatFlags.WordBreak);

            if (lines.Length > 1)
            {
                bounds.Y += TextRenderer.MeasureText(lines[0], bold).Height;
                TextRenderer.DrawText(e.Graphics, lines[1], packageView.Font, bounds, color, TextFormatFlags.WordBreak);
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
            var packages = packageRepository.GetPackages();
            switch (sortComboBox.SelectedIndex)
            {
                case 0: packages = packages.OrderByDescending(p => p.DownloadCount); break;
                case 1: packages = packages.OrderByDescending(p => p.Published); break;
                case 2: packages = packages.OrderBy(p => p.Title); break;
                case 3: packages = packages.OrderByDescending(p => p.Title); break;
                default: return;
            }

            packageView.Nodes.Clear();
            packageIcons.Images.Clear();

            var running = true;
            var stableOnly = releaseFilterComboBox.SelectedIndex == 0;
            packages.ToObservable()
                    .TakeWhile(p => running)
                    .Where(p => p.IsListed() && (!stableOnly || p.IsReleaseVersion()))
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOn(this)
                    .TakeWhile(p => packageView.Nodes.Count < 10)
                    .Subscribe(package => AddPackage(package), () => running = false);
        }
    }
}
