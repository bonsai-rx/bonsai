using System;
using Bonsai.Editor.Themes;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal class EditorToolWindow : DockContent
    {
        readonly IServiceProvider serviceProvider;
        readonly ThemeRenderer themeRenderer;

        private EditorToolWindow()
        {
        }

        protected EditorToolWindow(IServiceProvider provider)
        {
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            HideOnClose = true;
            DockAreas = DockAreas.Float |
                DockAreas.DockLeft |
                DockAreas.DockRight |
                DockAreas.DockTop |
                DockAreas.DockBottom;
        }

        protected IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
        }

        private void themeRenderer_ThemeChanged(object sender, EventArgs e)
        {
            InitializeTheme(themeRenderer);
        }

        protected virtual void InitializeTheme(ThemeRenderer themeRenderer)
        {
        }

        protected override string GetPersistString()
        {
            return GetType().Name;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (themeRenderer is not null)
            {
                themeRenderer.ThemeChanged += themeRenderer_ThemeChanged;
                InitializeTheme(themeRenderer);
            }
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (themeRenderer is not null)
                themeRenderer.ThemeChanged -= themeRenderer_ThemeChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
