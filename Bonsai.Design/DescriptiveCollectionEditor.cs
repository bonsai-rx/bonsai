using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;

namespace Bonsai.Design
{
    public class DescriptiveCollectionEditor : CollectionEditor
    {
        const float DefaultDpi = 96f;

        public DescriptiveCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            ActivateDescription(form);
            return form;
        }

        static void ActivateDescription(Control control)
        {
            var propertyGrid = control as System.Windows.Forms.PropertyGrid;
            if (propertyGrid != null)
            {
                propertyGrid.HelpVisible = true;
                using (var graphics = propertyGrid.CreateGraphics())
                {
                    var drawScale = graphics.DpiY / DefaultDpi *
                                    propertyGrid.Font.SizeInPoints / Control.DefaultFont.SizeInPoints;
                    PropertyGrid.ScaleDescriptionPanel(propertyGrid, new SizeF(drawScale, drawScale));
                }
            }

            foreach (Control child in control.Controls)
            {
                ActivateDescription(child);
            }
        }
    }
}
