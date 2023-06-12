using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    internal class Label : System.Windows.Forms.Label
    {
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if (EditorSettings.IsRunningOnMono)
            {
                var maximumSize = MaximumSize;
                MaximumSize = Size.Truncate(new SizeF(
                    maximumSize.Width * factor.Width,
                    maximumSize.Height * factor.Height));
            }

            base.ScaleControl(factor, specified);
        }
    }
}
