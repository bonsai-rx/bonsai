using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class ToolStripConstrainedStatusLabel : ToolStripStatusLabel
    {
        public Size MaximumSize { get; set; }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            var maximumSize = MaximumSize;
            var preferredSize = base.GetPreferredSize(constrainingSize);
            if (maximumSize.Width > 0) preferredSize.Width = Math.Min(preferredSize.Width, maximumSize.Width);
            if (maximumSize.Height > 0) preferredSize.Height = Math.Min(preferredSize.Height, maximumSize.Height);
            return preferredSize;
        }
    }
}
