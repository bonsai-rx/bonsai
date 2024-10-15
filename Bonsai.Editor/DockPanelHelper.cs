using System;
using System.Drawing;
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
                    dockState = dockContent.DockState;
                    floatWindowBounds = dockState == DockState.Float
                        ? dockContent.Pane.ParentForm.Bounds
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

                if (floatWindowBounds.HasValue)
                    dockContent.Show(dockPanel, floatWindowBounds.GetValueOrDefault());
                else
                {
                    dockContent.Show(dockPanel, dockState);
                    if (contentIndex >= 0 && dockContent.PanelPane is not null)
                        dockContent.PanelPane.SetContentIndex(dockContent, contentIndex);
                }
            };

            commandExecutor.Execute(
                createAndShow,
                close);
        }
    }
}
