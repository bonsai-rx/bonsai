using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Design;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    internal static class DockPanelHelper
    {
        public static void CreateDynamicContent<TDockContent>(
            this DockPanel dockPanel,
            Func<DockPanel, TDockContent> contentFactory,
            Action<TDockContent> contentClosed,
            DockState dockState,
            CommandExecutor commandExecutor)
            where TDockContent : DockContent
        {
            int contentIndex = -1;
            int dockPaneIndex = -1;
            int previousPaneIndex = -1;
            bool singletonContent = default;
            nint dockPaneHandle = default;
            nint previousPaneHandle = default;
            Rectangle? floatWindowBounds = null;
            TDockContent dockContent = default;
            DockAlignment? dockAlignment = default;
            double dockProportion = default;
            var isUndoAction = false;
            void CloseContent()
            {
                try
                {
                    isUndoAction = true;
                    dockContent.Close();
                }
                finally
                {
                    isUndoAction = false;
                }
            }

            void CreateAndShowContent()
            {
                dockContent = contentFactory(dockPanel);
                dockContent.HideOnClose = false;
                dockContent.FormClosed += (sender, e) =>
                {
                    contentClosed(dockContent);
                    var nestedDockingStatus = dockContent.Pane.NestedDockingStatus;
                    dockState = dockContent.DockState;
                    dockAlignment = nestedDockingStatus.Alignment;
                    dockProportion = nestedDockingStatus.Proportion;
                    dockPaneHandle = dockContent.Pane.Handle;
                    previousPaneHandle = (nestedDockingStatus.PreviousPane?.Handle).GetValueOrDefault();
                    dockPaneIndex = dockPanel.Panes.IndexOf(dockContent.Pane);
                    previousPaneIndex = dockPanel.Panes.IndexOf(nestedDockingStatus.PreviousPane);
                    singletonContent = dockContent.Pane.Contents.Count == 1;
                    floatWindowBounds = dockState == DockState.Float
                        ? dockContent.Pane.FloatWindow.Bounds
                        : null;
                    contentIndex = dockContent.Pane.Contents.IndexOf(dockContent);
                    if (!isUndoAction)
                    {
                        var isRedoAction = false;
                        commandExecutor.Execute(
                            () =>
                            {
                                if (isRedoAction)
                                    CloseContent();
                                else
                                    isRedoAction = true;
                            },
                            CreateAndShowContent);
                    }
                };

                var dockPane = GetPaneFromHandle(dockPaneHandle) ?? dockPanel.GetPaneFromIndex(dockPaneIndex);
                if (dockPane != null && !singletonContent)
                {
                    dockContent.Show(dockPane, contentIndex);
                    return;
                }

                contentIndex = -1;
                dockPaneIndex = -1;
                singletonContent = default;
                dockPane = GetPaneFromHandle(previousPaneHandle) ?? dockPanel.GetPaneFromIndex(previousPaneIndex);
                previousPaneIndex = -1;
                if (dockPane is DockPane previousPane)
                {
                    dockContent.Show(previousPane, dockAlignment.Value, dockProportion);
                }
                else if (floatWindowBounds.HasValue)
                {
                    dockContent.Show(dockPanel, floatWindowBounds.GetValueOrDefault());
                }
                else if (dockPanel.ActiveDocumentPane != null && dockState == DockState.Document)
                {
                    dockContent.Show(dockPanel.ActiveDocumentPane, contentIndex);
                }
                else
                {
                    dockContent.Show(dockPanel, dockState);
                }
            }

            commandExecutor.Execute(
                CreateAndShowContent,
                CloseContent);
        }

        public static void Show(this DockContent dockContent, DockPane pane, int contentIndex)
        {
            pane.DockPanel.SuspendLayout(allWindows: true);
            dockContent.DockPanel = pane.DockPanel;
            dockContent.Pane = pane;
            if (contentIndex >= 0 && contentIndex < pane.Contents.Count)
            {
                pane.SetContentIndex(dockContent, contentIndex);
            }
            dockContent.Show();
            pane.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
        }

        static DockPane GetPaneFromHandle(nint handle)
        {
            return Control.FromHandle(handle) as DockPane;
        }

        static DockPane GetPaneFromIndex(this DockPanel dockPanel, int index)
        {
            return index >= 0 && index < dockPanel.Panes.Count
                ? dockPanel.Panes[index]
                : null;
        }

        public static DockPane GetDocumentPane(this DockPanel dockPanel)
        {
            for (int i = 0; i < dockPanel.Panes.Count; i++)
            {
                var pane = dockPanel.Panes[i];
                if (pane.IsActiveDocumentPane)
                    return pane;
            }

            return null;
        }
    }
}
