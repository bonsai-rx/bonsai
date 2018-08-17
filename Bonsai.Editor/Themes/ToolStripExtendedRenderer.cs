using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ToolStripExtendedRenderer : ToolStripProfessionalRenderer
    {
        public ToolStripExtendedRenderer()
            : this(new ExtendedColorTable())
        {
        }

        public ToolStripExtendedRenderer(ExtendedColorTable extendedColorTable)
            : base(extendedColorTable)
        {
            ColorTable = extendedColorTable;
            RoundedEdges = false;
        }

        public new ExtendedColorTable ColorTable { get; private set; }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is StatusStrip) return;
            base.OnRenderToolStripBorder(e);
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            var dropDown = item as ToolStripDropDownItem;
            if (dropDown != null)
            {
                foreach (ToolStripItem dropItem in dropDown.DropDownItems)
                {
                    InitializeItem(dropItem);
                }
            }

            base.InitializeItem(item);
        }
    }
}
