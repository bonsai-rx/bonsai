﻿using System;
using System.IO;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;

namespace Bonsai.Editor
{
    public static class WorkflowExporter
    {
        public static void ExportImage(string fileName, string imageFileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.", nameof(fileName));
            }

            if (string.IsNullOrEmpty(imageFileName))
            {
                throw new ArgumentException("No output image file is specified.", nameof(imageFileName));
            }

            var workflowBuilder = ElementStore.LoadWorkflow(fileName);
            var iconRenderer = new SvgRendererFactory();
            var extension = Path.GetExtension(imageFileName);
            if (extension == ".svg")
            {
                var svg = ExportHelper.ExportSvg(workflowBuilder.Workflow, iconRenderer);
                File.WriteAllText(imageFileName, svg);
            }
            else
            {
                using var bitmap = ExportHelper.ExportBitmap(workflowBuilder.Workflow, Control.DefaultFont, iconRenderer);
                bitmap.Save(imageFileName);
            }
        }
    }
}
