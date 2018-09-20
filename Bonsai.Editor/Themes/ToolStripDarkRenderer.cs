using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ToolStripDarkRenderer : ToolStripExtendedRenderer
    {
        readonly Dictionary<Image, Image> itemImages = new Dictionary<Image, Image>();

        public ToolStripDarkRenderer()
            : base(new DarkColorTable())
        {
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            item.BackColor = ColorTable.ControlBackColor;
            item.ForeColor = ColorTable.ControlForeColor;
            base.InitializeItem(item);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ColorTable.ControlForeColor;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            Image image;
            if (!itemImages.TryGetValue(e.Image, out image))
            {
                image = ThemeHelper.Invert(e.Image);
                itemImages.Add(e.Image, image);
            }

            e = new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, image, e.ImageRectangle);
            base.OnRenderItemImage(e);
        }
    }
}
