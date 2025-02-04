using System;

namespace Bonsai.Design
{
    [Obsolete]
    public class WorkflowEditorSettings : VisualizerDialogSettings
    {
        public VisualizerDialogSettings EditorDialogSettings { get; set; }

        public VisualizerLayout EditorVisualizerLayout { get; set; }
    }
}
