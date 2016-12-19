using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class PropertyGrid : System.Windows.Forms.PropertyGrid
    {
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            ScaleDescriptionPanel(this, factor);
            base.ScaleControl(factor, specified);
        }

        internal static void ScaleDescriptionPanel(System.Windows.Forms.PropertyGrid propertyGrid, SizeF factor)
        {
            foreach (Control control in propertyGrid.Controls)
            {
                var controlType = control.GetType();
                if (controlType.Name == "DocComment")
                {
                    var userSizedField = controlType.BaseType.GetField(
                        "userSized",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    userSizedField.SetValue(control, true);
                    control.Height = (int)(control.Height * factor.Height);
                }
            }
        }
    }
}
