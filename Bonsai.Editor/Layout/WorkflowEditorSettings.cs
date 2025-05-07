using System;

namespace Bonsai.Design
{
    [Obsolete]
    public class WorkflowEditorSettings : VisualizerWindowSettings
    {
        public VisualizerWindowSettings EditorDialogSettings { get; set; }

        public VisualizerLayout EditorVisualizerLayout { get; set; }
    }
}
