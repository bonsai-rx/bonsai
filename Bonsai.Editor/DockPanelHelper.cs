using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Design;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor
{
    internal static class DockPanelHelper
    {
        public static void CreateDynamicContent<TDockPanel>(
            this TDockPanel dockPanel,
            Func<TDockPanel, DockContent> factory,
            DockState dockState,
            CommandExecutor commandExecutor)
            where TDockPanel : DockPanel
        {
            int contentIndex = -1;
            Rectangle? floatWindowBounds = null;
            DockContent dockContent = default;
            DockAlignment? dockAlignment = default;
            double dockProportion = default;
            IntPtr dockPaneHandle = default;
            IntPtr previousPaneHandle = default;
            var isUndoAction = false;
            Action close = () =>
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
            };

            Action createAndShow = null;
            createAndShow = () =>
            {
                dockContent = factory(dockPanel);
                dockContent.HideOnClose = false;
                dockContent.FormClosed += (sender, e) =>
                {
                    var nestedDockingStatus = dockContent.Pane.NestedDockingStatus;
                    dockState = dockContent.DockState;
                    dockAlignment = nestedDockingStatus.Alignment;
                    dockProportion = nestedDockingStatus.Proportion;
                    dockPaneHandle = dockContent.Pane.Handle;
                    previousPaneHandle = (nestedDockingStatus.PreviousPane?.Handle).GetValueOrDefault();
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
                                    close();
                                else
                                    isRedoAction = true;
                            },
                            createAndShow);
                    }
                };

                var dockControl = dockPaneHandle != IntPtr.Zero ? Control.FromHandle(dockPaneHandle) : null;
                if (dockControl is DockPane dockPane)
                {
                    dockContent.Show(dockPane, contentIndex);
                    return;
                }

                contentIndex = -1;
                dockControl = previousPaneHandle != IntPtr.Zero ? Control.FromHandle(previousPaneHandle) : null;
                if (dockControl is DockPane previousPane)
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
            };

            commandExecutor.Execute(
                createAndShow,
                close);
        }

        public static void Show(this DockContent dockContent, DockPane pane, int contentIndex)
        {
            pane.DockPanel.SuspendLayout(allWindows: true);
            dockContent.DockPanel = pane.DockPanel;
            dockContent.Pane = pane;
            pane.SetContentIndex(dockContent, contentIndex);
            dockContent.Show();
            pane.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
        }
    }
}
