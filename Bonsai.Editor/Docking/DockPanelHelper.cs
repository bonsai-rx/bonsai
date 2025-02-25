using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Bonsai.Design;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal static class DockPanelHelper
    {
        public static TDockContent CreateDynamicContent<TDockContent>(
            this DockPanel dockPanel,
            Func<DockPanel, TDockContent> contentFactory,
            Action<TDockContent> contentClosed,
            DockState dockState,
            CommandExecutor commandExecutor)
            where TDockContent : DockContent
        {
            int contentIndex = -1;
            bool singletonContent = false;
            object contentId = new();
            object dockPaneId = default;
            object previousPaneId = default;
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
                dockContent.Tag = contentId;
                dockContent.HideOnClose = false;
                dockContent.FormClosed += (sender, e) =>
                {
                    contentClosed(dockContent);
                    var nestedDockingStatus = dockContent.Pane.NestedDockingStatus;
                    dockState = dockContent.DockState;
                    dockAlignment = nestedDockingStatus.Alignment;
                    dockProportion = nestedDockingStatus.Proportion;
                    dockPaneId = dockContent.Pane.Tag;
                    previousPaneId = nestedDockingStatus.PreviousPane?.Tag;
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

                if (dockState == DockState.Unknown)
                    return;

                var dockPane = dockPanel.GetPaneFromId(dockPaneId);
                if (dockPane != null && !singletonContent)
                {
                    dockContent.Show(dockPane, contentIndex);
                    return;
                }

                contentIndex = -1;
                singletonContent = default;
                dockPane = dockPanel.GetPaneFromId(previousPaneId);
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

            if (dockState == DockState.Unknown)
                CreateAndShowContent();
            else
                commandExecutor.Execute(
                    CreateAndShowContent,
                    CloseContent);
            return dockContent;
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

        static DockPane GetPaneFromId(this DockPanel dockPanel, object paneId)
        {
            for (int i = 0; i < dockPanel.Panes.Count; i++)
            {
                if (ReferenceEquals(paneId, dockPanel.Panes[i].Tag))
                    return dockPanel.Panes[i];
            }

            return null;
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

        public static XDocument SaveAsXml(this DockPanel dockPanel)
        {
            using var memoryStream = new MemoryStream();
            dockPanel.SaveAsXml(memoryStream, Encoding.UTF8, upstream: true);
            memoryStream.Position = 0;
            return XDocument.Load(memoryStream);
        }
    }
}
