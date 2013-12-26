using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public class WorkflowEditorSettings : VisualizerDialogSettings
    {
        public VisualizerDialogSettings EditorDialogSettings { get; set; }

        public VisualizerLayout EditorVisualizerLayout { get; set; }
    }
}
