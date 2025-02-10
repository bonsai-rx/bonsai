using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    [ToolboxItem(false)]
    internal class WorkflowDockPaneStrip : DockPaneStripBase
    {
        private class TabWorkflow : Tab
        {
            public TabWorkflow(IDockContent content)
                : base(content)
            {
            }

            public int TabX { get; set; }

            public int TabWidth { get; set; }

            public int MaxWidth { get; set; }

            protected internal bool Flag { get; set; }
        }

        protected override Tab CreateTab(IDockContent content)
        {
            return new TabWorkflow(content);
        }

        [ToolboxItem(false)]
        private sealed class InertButton : InertButtonBase
        {
            public InertButton(Bitmap hovered, Bitmap normal, Bitmap pressed)
            {
                HoverImage = hovered;
                Image = normal;
                PressImage = pressed;
            }

            public override Bitmap Image { get; }

            public override Bitmap HoverImage { get; }

            public override Bitmap PressImage { get; }
        }

        #region Constants

        private const int DocumentStripGapTop = 0;
        private const int DocumentStripGapBottom = 1;
        private const int DocumentTabMaxWidth = 300;
        private const int DocumentButtonGapTop = 3;
        private const int DocumentButtonGapBottom = 3;
        private const int DocumentButtonGapBetween = 0;
        private const int DocumentButtonGapRight = 3;
        private const int DocumentTabGapTop = 0;//3;
        private const int DocumentTabGapLeft = 0;//3;
        private const int DocumentTabGapRight = 0;//3;
        private const int DocumentIconGapBottom = 2;//2;
        private const int DocumentIconGapLeft = 8;
        private const int DocumentIconGapRight = 0;
        private const int DocumentIconHeight = 16;
        private const int DocumentIconWidth = 16;
        private const int DocumentTextGapRight = 6;

        #endregion

        #region Members

        private ContextMenuStrip m_selectMenu;
        private InertButton m_buttonOverflow;
        private InertButton m_buttonWindowList;
        private IContainer m_components;
        private ToolTip m_toolTip;
        private Font m_font;
        private Font m_boldFont;
        private int m_startDisplayingTab = 0;
        private int m_endDisplayingTab = 0;
        private int m_firstDisplayingTab = 0;
        private bool m_documentTabsOverflow = false;
        private Rectangle _activeClose;
        private int _selectMenuMargin = 5;
        private bool m_suspendDrag = false;
        #endregion

        #region Properties

        private Rectangle TabStripRectangle
        {
            get
            {
                return TabStripRectangle_Document;
            }
        }

        private Rectangle TabStripRectangle_Document
        {
            get
            {
                Rectangle rect = ClientRectangle;
                return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width, rect.Height + DocumentStripGapTop - DocumentStripGapBottom);
            }
        }

        private Rectangle TabsRectangle
        {
            get
            {
                Rectangle rectWindow = TabStripRectangle;
                int x = rectWindow.X;
                int y = rectWindow.Y;
                int width = rectWindow.Width;
                int height = rectWindow.Height;

                x += DocumentTabGapLeft;
                width -= DocumentTabGapLeft +
                    DocumentTabGapRight +
                    DocumentButtonGapRight +
                    ButtonOverflow.Width +
                    ButtonWindowList.Width +
                    2 * DocumentButtonGapBetween;

                return new Rectangle(x, y, width, height);
            }
        }

        private ContextMenuStrip SelectMenu
        {
            get { return m_selectMenu; }
        }

        public int SelectMenuMargin
        {
            get { return _selectMenuMargin; }
            set { _selectMenuMargin = value; }
        }

        private InertButton ButtonOverflow
        {
            get
            {
                if (m_buttonOverflow == null)
                {
                    m_buttonOverflow = new InertButton(
                        DockPane.DockPanel.Theme.ImageService.DockPaneHover_OptionOverflow,
                        DockPane.DockPanel.Theme.ImageService.DockPane_OptionOverflow,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_OptionOverflow);
                    m_buttonOverflow.Click += new EventHandler(WindowList_Click);
                    Controls.Add(m_buttonOverflow);
                }

                return m_buttonOverflow;
            }
        }

        private InertButton ButtonWindowList
        {
            get
            {
                if (m_buttonWindowList == null)
                {
                    m_buttonWindowList = new InertButton(
                        DockPane.DockPanel.Theme.ImageService.DockPaneHover_List,
                        DockPane.DockPanel.Theme.ImageService.DockPane_List,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_List);
                    m_buttonWindowList.Click += new EventHandler(WindowList_Click);
                    Controls.Add(m_buttonWindowList);
                }

                return m_buttonWindowList;
            }
        }

        private static GraphicsPath _graphicsPath;
        internal static GraphicsPath GraphicsPath
        {
            get
            {
                if (_graphicsPath == null)
                    _graphicsPath = new GraphicsPath();

                return _graphicsPath;
            }
        }

        private IContainer Components
        {
            get { return m_components; }
        }

        public Font TextFont
        {
            get { return DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.TextFont; }
        }

        private Font BoldFont
        {
            get
            {
                if (IsDisposed)
                    return null;

                if (m_boldFont == null)
                {
                    m_font = TextFont;
                    m_boldFont = new Font(TextFont, FontStyle.Bold);
                }
                else if (m_font != TextFont)
                {
                    m_boldFont.Dispose();
                    m_font = TextFont;
                    m_boldFont = new Font(TextFont, FontStyle.Bold);
                }

                return m_boldFont;
            }
        }

        private int StartDisplayingTab
        {
            get { return m_startDisplayingTab; }
            set
            {
                m_startDisplayingTab = value;
                Invalidate();
            }
        }

        private int EndDisplayingTab
        {
            get { return m_endDisplayingTab; }
            set { m_endDisplayingTab = value; }
        }

        private int FirstDisplayingTab
        {
            get { return m_firstDisplayingTab; }
            set { m_firstDisplayingTab = value; }
        }

        private bool DocumentTabsOverflow
        {
            set
            {
                if (m_documentTabsOverflow == value)
                    return;

                m_documentTabsOverflow = value;
                SetInertButtons();
            }
        }

        #region Customizable Properties

        private TextFormatFlags DocumentTextFormat
        {
            get
            {
                TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.HorizontalCenter;
                if (RightToLeft == RightToLeft.Yes)
                    return textFormat | TextFormatFlags.RightToLeft;
                else
                    return textFormat;
            }
        }

        #endregion

        #endregion

        public WorkflowDockPaneStrip(DockPane pane)
            : base(pane)
        {
            SetStyle(ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            SuspendLayout();

            m_components = new Container();
            m_toolTip = new ToolTip(Components);
            m_selectMenu = new ContextMenuStrip(Components);
            pane.DockPanel.Theme.ApplyTo(m_selectMenu);

            ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Components.Dispose();
                if (m_boldFont != null)
                {
                    m_boldFont.Dispose();
                    m_boldFont = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override int MeasureHeight()
        {
            int height = Math.Max(TextFont.Height + DocumentTabGapTop + (PatchController.EnableHighDpi == true ? DocumentIconGapBottom : 0),
                ButtonOverflow.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
                + DocumentStripGapBottom + DocumentStripGapTop;

            return height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            CalculateTabs();
            if (DockPane.ActiveContent != null)
            {
                if (EnsureDocumentTabVisible(DockPane.ActiveContent, false))
                    CalculateTabs();
            }

            DrawTabStrip(e.Graphics);
        }

        protected override void OnRefreshChanges()
        {
            SetInertButtons();
            Invalidate();
        }

        public override GraphicsPath GetOutline(int index)
        {
            Rectangle rectTab = Tabs[index].Rectangle.Value;
            rectTab.X -= rectTab.Height / 2;
            rectTab.Intersect(TabsRectangle);
            rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = GetTabOutline(Tabs[index], true, true);
            path.AddPath(pathTab, true);

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
                path.AddLine(rectTab.Right, rectTab.Top, rectPaneClient.Right, rectTab.Top);
                path.AddLine(rectPaneClient.Right, rectTab.Top, rectPaneClient.Right, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Left, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Left, rectTab.Top);
                path.AddLine(rectPaneClient.Left, rectTab.Top, rectTab.Right, rectTab.Top);
            }
            else
            {
                path.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
                path.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
                path.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
            }
            return path;
        }

        private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
        {
            bool overflow = false;

            var tab = Tabs[index] as TabWorkflow;
            tab.MaxWidth = GetMaxTabWidth(index);
            int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
            if (x + width < rectTabStrip.Right || index == StartDisplayingTab)
            {
                tab.TabX = x;
                tab.TabWidth = width;
                EndDisplayingTab = index;
            }
            else
            {
                tab.TabX = 0;
                tab.TabWidth = 0;
                overflow = true;
            }
            x += width;

            return overflow;
        }

        /// <summary>
        /// Calculate which tabs are displayed and in what order.
        /// </summary>
        private void CalculateTabs()
        {
            if (m_startDisplayingTab >= Tabs.Count)
                m_startDisplayingTab = 0;

            Rectangle rectTabStrip = TabsRectangle;

            int x = rectTabStrip.X; //+ rectTabStrip.Height / 2;
            bool overflow = false;

            // Originally all new documents that were considered overflow
            // (not enough pane strip space to show all tabs) were added to
            // the far left (assuming not right to left) and the tabs on the
            // right were dropped from view. If StartDisplayingTab is not 0
            // then we are dealing with making sure a specific tab is kept in focus.
            if (m_startDisplayingTab > 0)
            {
                int tempX = x;
                var tab = Tabs[m_startDisplayingTab] as TabWorkflow;
                tab.MaxWidth = GetMaxTabWidth(m_startDisplayingTab);

                // Add the active tab and tabs to the left
                for (int i = StartDisplayingTab; i >= 0; i--)
                    CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // Store which tab is the first one displayed so that it
                // will be drawn correctly (without part of the tab cut off)
                FirstDisplayingTab = EndDisplayingTab;

                tempX = x; // Reset X location because we are starting over

                // Start with the first tab displayed - name is a little misleading.
                // Loop through each tab and set its location. If there is not enough
                // room for all of them overflow will be returned.
                for (int i = EndDisplayingTab; i < Tabs.Count; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // If not all tabs are shown then we have an overflow.
                if (FirstDisplayingTab != 0)
                    overflow = true;
            }
            else
            {
                for (int i = StartDisplayingTab; i < Tabs.Count; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
                for (int i = 0; i < StartDisplayingTab; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);

                FirstDisplayingTab = StartDisplayingTab;
            }

            if (!overflow)
            {
                m_startDisplayingTab = 0;
                FirstDisplayingTab = 0;
                x = rectTabStrip.X;
                foreach (TabWorkflow tab in Tabs)
                {
                    tab.TabX = x;
                    x += tab.TabWidth;
                }
            }

            DocumentTabsOverflow = overflow;
        }

        protected override void EnsureTabVisible(IDockContent content)
        {
            if (!Tabs.Contains(content))
                return;

            CalculateTabs();
            EnsureDocumentTabVisible(content, true);
        }

        private bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
        {
            int index = Tabs.IndexOf(content);
            if (index == -1) // TODO: should prevent it from being -1;
                return false;

            var tab = Tabs[index] as TabWorkflow;
            if (tab.TabWidth != 0)
                return false;

            StartDisplayingTab = index;
            if (repaint)
                Invalidate();

            return true;
        }

        private const int TAB_CLOSE_BUTTON_WIDTH = 30;

        private int GetMaxTabWidth(int index)
        {
            IDockContent content = Tabs[index].Content;
            int height = GetTabRectangle(index).Height;
            Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont, new Size(DocumentTabMaxWidth, height), DocumentTextFormat);

            int width;
            if (DockPane.DockPanel.ShowDocumentIcon)
                width = sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight;
            else
                width = sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;

            width += TAB_CLOSE_BUTTON_WIDTH;
            return width;
        }

        private void DrawTabStrip(Graphics g)
        {
            // IMPORTANT: fill background.
            Rectangle rectTabStrip = TabStripRectangle;
            g.FillRectangle(DockPane.DockPanel.Theme.PaintingService.GetBrush(DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background), rectTabStrip);
            DrawTabStrip_Document(g);
        }

        private void DrawTabStrip_Document(Graphics g)
        {
            int count = Tabs.Count;
            if (count == 0)
                return;

            Rectangle rectTabStrip = new Rectangle(TabStripRectangle.Location, TabStripRectangle.Size);
            rectTabStrip.Height += 1;

            // Draw the tabs
            Rectangle rectTabOnly = TabsRectangle;
            Rectangle rectTab = Rectangle.Empty;
            TabWorkflow tabActive = null;
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            for (int i = 0; i < count; i++)
            {
                rectTab = GetTabRectangle(i);
                if (Tabs[i].Content == DockPane.ActiveContent)
                {
                    tabActive = Tabs[i] as TabWorkflow;
                    tabActive.Rectangle = rectTab;
                    continue;
                }

                if (rectTab.IntersectsWith(rectTabOnly))
                {
                    var tab = Tabs[i] as TabWorkflow;
                    tab.Rectangle = rectTab;
                    DrawTab(g, tab);
                }
            }

            g.SetClip(rectTabStrip);

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
            }
            else
            {
                Color tabUnderLineColor;
                if (tabActive != null && DockPane.IsActiveDocumentPane)
                    tabUnderLineColor = DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background;
                else
                    tabUnderLineColor = DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background;

                g.DrawLine(DockPane.DockPanel.Theme.PaintingService.GetPen(tabUnderLineColor, 4), rectTabStrip.Left, rectTabStrip.Bottom, rectTabStrip.Right, rectTabStrip.Bottom);
            }

            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            if (tabActive != null)
            {
                rectTab = tabActive.Rectangle.Value;
                if (rectTab.IntersectsWith(rectTabOnly))
                {
                    rectTab.Intersect(rectTabOnly);
                    tabActive.Rectangle = rectTab;
                    DrawTab(g, tabActive);
                }
            }
        }

        private Rectangle GetTabRectangle(int index)
        {
            Rectangle rectTabStrip = TabStripRectangle;
            var tab = (TabWorkflow)Tabs[index];

            Rectangle rect = new Rectangle();
            rect.X = tab.TabX;
            rect.Width = tab.TabWidth;
            rect.Height = rectTabStrip.Height - DocumentTabGapTop;

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                rect.Y = rectTabStrip.Y + DocumentStripGapBottom;
            else
                rect.Y = rectTabStrip.Y + DocumentTabGapTop;

            return rect;
        }

        private GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
        {
            GraphicsPath.Reset();
            Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));

            // Shorten TabOutline so it doesn't get overdrawn by icons next to it
            rect.Intersect(TabsRectangle);
            rect.Width--;

            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = RectangleToScreen(rect);

            GraphicsPath.AddRectangle(rect);
            return GraphicsPath;
        }

        private void DrawTab(Graphics g, TabWorkflow tab)
        {
            var rect = tab.Rectangle.Value;
            if (tab.TabWidth == 0)
                return;

            var rectCloseButton = GetCloseButtonRect(rect);
            Rectangle rectIcon = new Rectangle(
                rect.X + DocumentIconGapLeft,
                rect.Y + rect.Height - DocumentIconGapBottom - DocumentIconHeight,
                DocumentIconWidth, DocumentIconHeight);
            Rectangle rectText = PatchController.EnableHighDpi == true
                ? new Rectangle(
                    rect.X + DocumentIconGapLeft,
                    rect.Y + rect.Height - DocumentIconGapBottom - TextFont.Height,
                    DocumentIconWidth, TextFont.Height)
                : rectIcon;
            if (DockPane.DockPanel.ShowDocumentIcon)
            {
                rectText.X += rectIcon.Width + DocumentIconGapRight;
                rectText.Y = rect.Y;
                rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft - DocumentIconGapRight - DocumentTextGapRight - rectCloseButton.Width;
                rectText.Height = rect.Height;
            }
            else
                rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight - rectCloseButton.Width;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
            rectBack.Width += DocumentIconGapLeft;
            rectBack.X -= DocumentIconGapLeft;

            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);

            Color activeColor = DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background;
            Color lostFocusColor = DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background;
            Color inactiveColor = DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background;
            Color mouseHoverColor = DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Background;

            Color activeText = DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Text;
            Color lostFocusText = DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Text;
            Color inactiveText = DockPane.DockPanel.Theme.ColorPalette.TabUnselected.Text;
            Color mouseHoverText = DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Text;

            Color text;
            Image image = null;
            Color paint;
            var imageService = DockPane.DockPanel.Theme.ImageService;
            if (DockPane.ActiveContent == tab.Content)
            {
                paint = activeColor;
                text = activeText;
                image = IsMouseDown
                    ? imageService.TabPressActive_Close
                    : rectCloseButton == ActiveClose
                        ? imageService.TabHoverActive_Close
                        : imageService.TabActive_Close;
            }
            else
            {
                if (tab.Content == DockPane.MouseOverTab)
                {
                    paint = mouseHoverColor;
                    text = mouseHoverText;
                    image = IsMouseDown
                        ? imageService.TabPressInactive_Close
                        : rectCloseButton == ActiveClose
                            ? imageService.TabHoverInactive_Close
                            : imageService.TabInactive_Close;
                }
                else
                {
                    paint = inactiveColor;
                    text = inactiveText;
                }
            }

            g.FillRectangle(DockPane.DockPanel.Theme.PaintingService.GetBrush(paint), rect);
            TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, text, DocumentTextFormat);
            if (image != null)
                g.DrawImage(image, rectCloseButton);

            if (rectTab.Contains(rectIcon) && DockPane.DockPanel.ShowDocumentIcon)
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        private bool m_isMouseDown = false;
        protected bool IsMouseDown
        {
            get { return m_isMouseDown; }
            private set
            {
                if (m_isMouseDown == value)
                    return;

                m_isMouseDown = value;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsMouseDown)
                IsMouseDown = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // suspend drag if mouse is down on active close button.
            this.m_suspendDrag = ActiveCloseHitTest(e.Location);
            if (!IsMouseDown)
                IsMouseDown = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.m_suspendDrag)
                base.OnMouseMove(e);

            int index = HitTest(PointToClient(MousePosition));
            string toolTip = string.Empty;

            bool tabUpdate = false;
            bool buttonUpdate = false;
            if (index != -1)
            {
                var tab = Tabs[index] as TabWorkflow;
                tabUpdate = SetMouseOverTab(tab.Content == DockPane.ActiveContent ? null : tab.Content);

                if (!String.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
                    toolTip = tab.Content.DockHandler.ToolTipText;
                else if (tab.MaxWidth > tab.TabWidth)
                    toolTip = tab.Content.DockHandler.TabText;

                var mousePos = PointToClient(MousePosition);
                var tabRect = tab.Rectangle.Value;
                var closeButtonRect = GetCloseButtonRect(tabRect);
                var mouseRect = new Rectangle(mousePos, new Size(1, 1));
                buttonUpdate = SetActiveClose(closeButtonRect.IntersectsWith(mouseRect) ? closeButtonRect : Rectangle.Empty);
            }
            else
            {
                tabUpdate = SetMouseOverTab(null);
                buttonUpdate = SetActiveClose(Rectangle.Empty);
            }

            if (tabUpdate || buttonUpdate)
                Invalidate();

            if (m_toolTip.GetToolTip(this) != toolTip)
            {
                m_toolTip.Active = false;
                m_toolTip.SetToolTip(this, toolTip);
                m_toolTip.Active = true;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left)
                return;

            var indexHit = HitTest();
            if (indexHit > -1)
                TabCloseButtonHit(indexHit);
        }

        private void TabCloseButtonHit(int index)
        {
            var mousePos = PointToClient(MousePosition);
            var tabRect = GetTabBounds(Tabs[index]);
            if (tabRect.Contains(ActiveClose) && ActiveCloseHitTest(mousePos))
                TryCloseTab(index);
        }

        private Rectangle GetCloseButtonRect(Rectangle rectTab)
        {
            const int gap = 3;
            var imageSize = PatchController.EnableHighDpi == true ? rectTab.Height - gap * 2 : 15;
            return new Rectangle(rectTab.X + rectTab.Width - imageSize - gap - 1, rectTab.Y + gap, imageSize, imageSize);
        }

        private void WindowList_Click(object sender, EventArgs e)
        {
            SelectMenu.Items.Clear();
            foreach (TabWorkflow tab in Tabs)
            {
                IDockContent content = tab.Content;
                ToolStripItem item = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
                item.Tag = tab.Content;
                item.Click += new EventHandler(ContextMenuItem_Click);
            }

            var workingArea = Screen.GetWorkingArea(ButtonWindowList.PointToScreen(new Point(ButtonWindowList.Width / 2, ButtonWindowList.Height / 2)));
            var menu = new Rectangle(ButtonWindowList.PointToScreen(new Point(0, ButtonWindowList.Location.Y + ButtonWindowList.Height)), SelectMenu.Size);
            var menuMargined = new Rectangle(menu.X - SelectMenuMargin, menu.Y - SelectMenuMargin, menu.Width + SelectMenuMargin, menu.Height + SelectMenuMargin);
            if (workingArea.Contains(menuMargined))
            {
                SelectMenu.Show(menu.Location);
            }
            else
            {
                var newPoint = menu.Location;
                newPoint.X = DrawHelper.Balance(SelectMenu.Width, SelectMenuMargin, newPoint.X, workingArea.Left, workingArea.Right);
                newPoint.Y = DrawHelper.Balance(SelectMenu.Size.Height, SelectMenuMargin, newPoint.Y, workingArea.Top, workingArea.Bottom);
                var button = ButtonWindowList.PointToScreen(new Point(0, ButtonWindowList.Height));
                if (newPoint.Y < button.Y)
                {
                    // flip the menu up to be above the button.
                    newPoint.Y = button.Y - ButtonWindowList.Height;
                    SelectMenu.Show(newPoint, ToolStripDropDownDirection.AboveRight);
                }
                else
                {
                    SelectMenu.Show(newPoint);
                }
            }
        }

        private void ContextMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                IDockContent content = (IDockContent)item.Tag;
                DockPane.ActiveContent = content;
            }
        }

        private void SetInertButtons()
        {
            ButtonOverflow.Visible = m_documentTabsOverflow;
            ButtonOverflow.RefreshChanges();

            ButtonWindowList.Visible = !m_documentTabsOverflow;
            ButtonWindowList.RefreshChanges();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            LayoutButtons();
            OnRefreshChanges();
            base.OnLayout(levent);
        }

        private void LayoutButtons()
        {
            Rectangle rectTabStrip = TabStripRectangle;

            // Set position and size of the buttons
            int buttonWidth = ButtonOverflow.Image.Width;
            int buttonHeight = ButtonOverflow.Image.Height;
            int height = rectTabStrip.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * height / buttonHeight;
                buttonHeight = height;
            }
            Size buttonSize = new Size(buttonWidth, buttonHeight);

            int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
                - DocumentButtonGapRight - buttonWidth;
            int y = rectTabStrip.Y + DocumentButtonGapTop;
            Point point = new Point(x, y);
            ButtonOverflow.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            // If the close button is not visible draw the window list button overtop.
            // Otherwise it is drawn to the left of the close button.
            ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
        }

        private void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
            if (PatchController.EnableMemoryLeakFix == true)
            {
                ContentClosed();
            }
        }

        protected override int HitTest(Point point)
        {
            if (!TabsRectangle.Contains(point))
                return -1;

            foreach (Tab tab in Tabs)
            {
                GraphicsPath path = GetTabOutline(tab, true, false);
                if (path.IsVisible(point))
                    return Tabs.IndexOf(tab);
            }

            return -1;
        }

        protected override bool MouseDownActivateTest(MouseEventArgs e)
        {
            bool result = base.MouseDownActivateTest(e);
            if (result && (e.Button == MouseButtons.Left))
            {
                // don't activate if mouse is down on active close button
                result = !ActiveCloseHitTest(e.Location);
            }
            return result;
        }

        private bool ActiveCloseHitTest(Point ptMouse)
        {
            bool result = false;
            if (!ActiveClose.IsEmpty)
            {
                var mouseRect = new Rectangle(ptMouse, new Size(1, 1));
                result = ActiveClose.IntersectsWith(mouseRect);
            }
            return result;
        }

        protected override Rectangle GetTabBounds(Tab tab)
        {
            GraphicsPath path = GetTabOutline(tab, true, false);
            RectangleF rectangle = path.GetBounds();
            return new Rectangle((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height);
        }

        private Rectangle ActiveClose
        {
            get { return _activeClose; }
        }

        private bool SetActiveClose(Rectangle rectangle)
        {
            if (_activeClose == rectangle)
                return false;

            _activeClose = rectangle;
            return true;
        }

        private bool SetMouseOverTab(IDockContent content)
        {
            if (DockPane.MouseOverTab == content)
                return false;

            DockPane.MouseOverTab = content;
            return true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            var tabUpdate = SetMouseOverTab(null);
            var buttonUpdate = SetActiveClose(Rectangle.Empty);
            if (tabUpdate || buttonUpdate)
                Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }
    }
}
