using Bonsai.NuGet.Design.Properties;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    class PackageView : TreeView
    {
        const int WM_NOTIFY = 0x004e;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONDBLCLK = 0x0203;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONDBLCLK = 0x0206;
        const int TVS_NOHSCROLL = 0x8000;
        const int TVM_SETEXTENDEDSTYLE = 0x112C;
        const int TVS_EX_DOUBLEBUFFER = 0x0004;

        Font boldFont;
        Font titleFont;
        Font hyperlinkFont;
        Font iconFont;
        Brush nodeHighlight;
        int boundsMargin;
        SizeF buttonSize;
        int verticalScrollBarWidth;
        PackageOperationType operation;
        TreeNode operationHoverNode;
        bool operationButtonState;
        const int DefaultBoundsMargin = 6;
        static readonly object OperationClickEvent = new();

        public PackageView()
        {
            ShowLines = false;
            HotTracking = true;
            FullRowSelect = true;
            DrawMode = TreeViewDrawMode.OwnerDrawAll;
            boundsMargin = DefaultBoundsMargin;
            verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            nodeHighlight = new SolidBrush(ControlPaint.LightLight(SystemColors.Highlight));
            SetStyle(ControlStyles.ResizeRedraw, false);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode
        {
            get => base.DrawMode;
            set => base.DrawMode = value;
        }

        [Category("Action")]
        public event PackageViewEventHandler OperationClick
        {
            add { Events.AddHandler(OperationClickEvent, value); }
            remove { Events.RemoveHandler(OperationClickEvent, value); }
        }

        private Image OperationImage { get; set; }

        public PackageOperationType Operation
        {
            get => operation;
            set
            {
                operation = value;
                OperationImage = operation switch
                {
                    PackageOperationType.Open => Resources.OpenImage,
                    PackageOperationType.Update => Resources.PackageUpdateImage,
                    PackageOperationType.Uninstall => Resources.PackageRemoveImage,
                    PackageOperationType.Install or _ => Resources.DownloadImage
                };
            }
        }

        public bool CanSelectNodes { get; set; }

        private void OnOperationClick(PackageViewEventArgs e)
        {
            (Events[OperationClickEvent] as PackageViewEventHandler)?.Invoke(this, e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.Style |= TVS_NOHSCROLL;
                return parameters;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            // slightly dampened scale factor for right margin
            var widthScaleFactor = factor.Height * 0.5f + 0.5f;
            verticalScrollBarWidth = (int)(SystemInformation.VerticalScrollBarWidth * widthScaleFactor);
            boundsMargin = (int)(DefaultBoundsMargin * factor.Height);
            using var graphics = CreateGraphics();
            buttonSize = graphics.GetImageSize(OperationImage);
            base.ScaleControl(factor, specified);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!NativeMethods.IsRunningOnMono)
            {
                NativeMethods.SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            }
            boldFont = new Font(Font, FontStyle.Bold);
            titleFont = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Bold);
            hyperlinkFont = new Font(Font, FontStyle.Bold | FontStyle.Underline);
            iconFont = new Font(Font.FontFamily, Font.Size - 1.5f);
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NOTIFY:
                    return;
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDBLCLK:
                    if (!CanSelectNodes) return;
                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (CanSelectNodes)
            {
                var node = GetNodeAt(e.Location);
                if (node != null)
                {
                    SelectedNode = node;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (operationHoverNode != node)
                operationButtonState = false;

            if (node != null)
            {
                var buttonBounds = GetOperationButtonBounds(node.Bounds);
                var hoverState = buttonBounds.Contains(e.Location);
                if (operationButtonState != hoverState)
                {
                    var nodeBounds = node.Bounds;
                    nodeBounds.Width = Width - nodeBounds.X;
                    Invalidate(nodeBounds);
                }

                operationButtonState = hoverState;
            }

            operationHoverNode = node;
            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left &&
                OperationHitTest(e.Location, out TreeViewHitTestInfo hitTestInfo))
            {
                OnOperationClick(new PackageViewEventArgs(
                    (IPackageSearchMetadata)hitTestInfo.Node.Tag,
                    Operation));
            }
            base.OnMouseClick(e);
        }

        private bool OperationHitTest(Point pt, out TreeViewHitTestInfo hitTestInfo)
        {
            hitTestInfo = HitTest(pt);
            if (hitTestInfo.Node != null && !hitTestInfo.Node.Checked)
            {
                if (hitTestInfo.Node.Tag == null) return false;
                var buttonBounds = GetOperationButtonBounds(hitTestInfo.Node.Bounds);
                return buttonBounds.Contains(pt);
            }

            return false;
        }

        private int RightMargin
        {
            get { return Width - verticalScrollBarWidth; }
        }

        private RectangleF GetOperationButtonBounds(Rectangle nodeBounds)
        {
            // Node bounds span the full text area but we need to account for scroll bar
            return new(
                x: RightMargin - buttonSize.Width - boundsMargin,
                y: nodeBounds.Y + boundsMargin,
                width: buttonSize.Width,
                height: buttonSize.Height);
        }

        private void FillImageBounds(Graphics graphics, Brush brush, Image image, ref Rectangle bounds)
        {
            var imageSize = graphics.GetImageSize(image);
            RectangleF imageBounds = new(
                x: bounds.Right - imageSize.Width,
                y: bounds.Y,
                width: imageSize.Width,
                height: imageSize.Height);
            graphics.FillRectangle(brush, imageBounds);
        }

        private void DrawImageOverlay(Graphics graphics, Image overlay, float imageX, float imageY)
        {
            var imageSize = graphics.GetImageSize(overlay);
            var overlayX = imageX + ImageList.ImageSize.Width - imageSize.Width / 2 - Margin.Horizontal;
            var overlayY = imageY + ImageList.ImageSize.Height - imageSize.Height / 2 - Margin.Vertical;
            graphics.DrawImage(overlay, overlayX, overlayY);
        }

        private void DrawInlineImage(Graphics graphics, Image image, ref Rectangle bounds)
        {
            float imageX;
            var imageSize = Size.Round(graphics.GetImageSize(image));
            imageX = bounds.Right - imageSize.Width;
            graphics.DrawImage(image, imageX, bounds.Y);
            bounds.Width -= imageSize.Width + boundsMargin;
        }

        private void DrawInlineText(
            Graphics graphics,
            string text,
            Font font,
            Color color,
            ref Rectangle bounds)
        {
            if (bounds.Width <= 0)
                return;

            var textFormatFlags = TextFormatFlags.NoPadding;
            var textSize = TextRenderer.MeasureText(graphics, text, font, bounds.Size, textFormatFlags);
            if (textSize.Width > bounds.Width)
            {
                textFormatFlags |= TextFormatFlags.WordEllipsis;
            }

            TextRenderer.DrawText(graphics, text, font, bounds, color, textFormatFlags);
            bounds.X += textSize.Width;
            bounds.Width -= textSize.Width;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = false;
            var bounds = e.Bounds;
            bounds.Width = RightMargin - bounds.X;
            var color = (e.State & TreeNodeStates.Selected) != 0
                ? SystemColors.HighlightText
                : SystemColors.WindowText;
            if (e.Node.Tag == null)
            {
                TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, bounds, color,
                                      TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter);
            }
            else
            {
                var nodeHot = (e.State & TreeNodeStates.Hot) != 0;
                var nodeSelected = (e.State & TreeNodeStates.Selected) != 0;
                if (nodeSelected)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
                }

                if (!nodeSelected && nodeHot)
                {
                    e.Graphics.FillRectangle(nodeHighlight, bounds);
                }

                // Get source and local package info
                var packageMetadata = (IPackageSearchMetadata)e.Node.Tag;
                var localPackageNode = e.Node.Nodes.Count > 0 ? e.Node.Nodes[Resources.UpdatesNodeName] : null;
                var localPackageMetadata = (LocalPackageInfo)localPackageNode?.Tag;

                // Draw package icon
                var iconText = Resources.PrereleaseLabel;
                var iconTextFlags = TextFormatFlags.NoPadding;
                var iconTextSize = TextRenderer.MeasureText(e.Graphics, iconText, iconFont, bounds.Size, iconTextFlags);
                var iconImageX = e.Bounds.X + Margin.Left;
                var iconImageY = e.Bounds.Top + (e.Bounds.Height - ImageList.ImageSize.Height - iconTextSize.Height) / 2;
                var iconImageIndex = ImageList.Images.IndexOfKey(e.Node.ImageKey);
                if (iconImageIndex >= 0)
                {
                    ImageList.Draw(e.Graphics, iconImageX, iconImageY, iconImageIndex);
                    if (localPackageMetadata != null)
                    {
                        var iconOverlay = localPackageMetadata.Identity.Version < packageMetadata.Identity.Version
                            ? Resources.PackageUpdateImage
                            : Resources.PackageInstalledImage;
                        DrawImageOverlay(e.Graphics, iconOverlay, iconImageX, iconImageY);
                    }

                    if (packageMetadata.Identity.Version.IsPrerelease)
                    {
                        var iconTextPosition = new Point(
                            x: iconImageX,
                            y: iconImageY + ImageList.ImageSize.Height);
                        TextRenderer.DrawText(e.Graphics, iconText, iconFont, iconTextPosition, color, iconTextFlags);
                    }
                }
                bounds.X += iconTextSize.Width + Margin.Horizontal;
                bounds.Width -= iconTextSize.Width + Margin.Horizontal;

                // Add spacing between text boxes
                bounds.Y += boundsMargin;
                bounds.Height -= boundsMargin;

                // Draw operation image
                bounds.Width -= boundsMargin;
                if (nodeHot && OperationImage != null)
                {
                    var mousePosition = PointToClient(MousePosition);
                    var buttonBounds = GetOperationButtonBounds(e.Node.Bounds);
                    if (buttonBounds.Contains(mousePosition))
                    {
                        FillImageBounds(
                            e.Graphics,
                            SystemBrushes.ButtonHighlight,
                            OperationImage,
                            ref bounds);
                    }
                    DrawInlineImage(e.Graphics, OperationImage, ref bounds);
                }
                else
                    bounds.Width -= Size.Round(e.Graphics.GetImageSize(OperationImage)).Width + boundsMargin;

                // Draw package version
                var packageVersion = packageMetadata.Identity.Version.ToString();
                var textSize = TextRenderer.MeasureText(e.Graphics, packageVersion, Font);
                var textPosition = new Point(bounds.Right - textSize.Width - boundsMargin, bounds.Y);
                TextRenderer.DrawText(e.Graphics, packageVersion, Font, textPosition, color);
                bounds.Width -= textSize.Width + boundsMargin;

                // Draw package warnings
                var warningNode = e.Node.Nodes.Count > 0 ? e.Node.Nodes[Resources.PackageWarningKey] : null;
                if (warningNode != null)
                    DrawInlineImage(e.Graphics, Resources.WarningImage, ref bounds);

                // Draw package title
                var titleBounds = bounds;
                var titleTextFlags = TextFormatFlags.Default;
                var lines = e.Node.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var titleSize = TextRenderer.MeasureText(e.Graphics, lines[0], titleFont, bounds.Size, titleTextFlags);
                TextRenderer.DrawText(e.Graphics, lines[0], titleFont, titleBounds, color, titleTextFlags);
                titleBounds.X += titleSize.Width;
                titleBounds.Width -= titleSize.Width;
                if (packageMetadata.PrefixReserved)
                {
                    var image = Resources.PrefixReservedMediumImage;
                    var imageSize = Size.Round(e.Graphics.GetImageSize(image));
                    var imagePadding = imageSize.Width / 8;
                    titleBounds.X -= imagePadding;
                    var imageY = titleBounds.Y + titleFont.GetHeight(e.Graphics) - imageSize.Height + imagePadding;
                    e.Graphics.DrawImage(image, titleBounds.X, imageY);
                    titleBounds.X += imageSize.Width + imagePadding;
                    titleBounds.Width -= imageSize.Width;
                }

                // Draw package authors and download count
                titleBounds.Y += titleSize.Height - textSize.Height;
                var hasDownloadCount = packageMetadata.DownloadCount.HasValue;
                var downloadSeparator = hasDownloadCount ? ", " : string.Empty;
                DrawInlineText(e.Graphics, $"by {packageMetadata.Authors}{downloadSeparator}", Font, color, ref titleBounds);
                if (hasDownloadCount)
                {
                    var downloadCount = packageMetadata.DownloadCount.GetValueOrDefault();
                    DrawInlineText(e.Graphics, FormatHelper.ToLargeSuffix(downloadCount), boldFont, color, ref titleBounds);
                    DrawInlineText(e.Graphics, " downloads", Font, color, ref titleBounds);
                }

                // Line break after title
                bounds.Y += titleSize.Height;
                bounds.Height -= titleSize.Height;

                // Measure package deprecation notice
                var deprecationSize = Size.Empty;
                var deprecationMetadata = warningNode?.Tag as PackageDeprecationMetadata;
                if (deprecationMetadata != null)
                {
                    var notice = Resources.PackageDeprecationNotice;
                    deprecationSize = TextRenderer.MeasureText(notice, boldFont, bounds.Size, TextFormatFlags.WordEllipsis);
                    bounds.Height -= deprecationSize.Height;
                }

                // Draw package description
                if (lines.Length > 1)
                {
                    const TextFormatFlags DescriptionFlags =
                        TextFormatFlags.TextBoxControl
                        | TextFormatFlags.WordBreak
                        | TextFormatFlags.EndEllipsis;
                    var descriptionSize = TextRenderer.MeasureText(e.Graphics, lines[1], Font, bounds.Size, DescriptionFlags);
                    TextRenderer.DrawText(e.Graphics, lines[1], Font, bounds, color, DescriptionFlags);
                    bounds.Y += descriptionSize.Height;
                    bounds.Height -= descriptionSize.Height;
                }

                // Draw package deprecation message
                if (deprecationMetadata != null)
                {
                    bounds.Height += deprecationSize.Height;
                    var notice = Resources.PackageDeprecationNotice;
                    DrawInlineText(e.Graphics, notice, boldFont, color, ref bounds);
                    if (deprecationMetadata.AlternatePackage != null && bounds.Width > 0)
                    {
                        var alternatePackageId = deprecationMetadata.AlternatePackage.PackageId;
                        var alternateNoticeParts = Resources.PackageDeprecationAlternateNotice.Split('|');
                        DrawInlineText(e.Graphics, alternateNoticeParts[0], boldFont, color, ref bounds);
                        DrawInlineText(e.Graphics, alternatePackageId, hyperlinkFont, color, ref bounds);
                        DrawInlineText(e.Graphics, alternateNoticeParts[1], boldFont, color, ref bounds);
                    }
                }
            }

            base.OnDrawNode(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && nodeHighlight != null)
            {
                nodeHighlight.Dispose();
                nodeHighlight = null;
            }

            base.Dispose(disposing);
        }
    }
}
