using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class MapColorTable : ExtendedColorTable
    {
        readonly Color buttonCheckedGradientBegin;
        readonly Color buttonCheckedGradientEnd;
        readonly Color buttonCheckedGradientMiddle;
        readonly Color buttonCheckedHighlight;
        readonly Color buttonCheckedHighlightBorder;
        readonly Color buttonPressedBorder;
        readonly Color buttonPressedGradientBegin;
        readonly Color buttonPressedGradientEnd;
        readonly Color buttonPressedGradientMiddle;
        readonly Color buttonPressedHighlight;
        readonly Color buttonPressedHighlightBorder;
        readonly Color buttonSelectedBorder;
        readonly Color buttonSelectedGradientBegin;
        readonly Color buttonSelectedGradientEnd;
        readonly Color buttonSelectedGradientMiddle;
        readonly Color buttonSelectedHighlight;
        readonly Color buttonSelectedHighlightBorder;
        readonly Color checkBackground;
        readonly Color checkPressedBackground;
        readonly Color checkSelectedBackground;
        readonly Color gripDark;
        readonly Color gripLight;
        readonly Color imageMarginGradientBegin;
        readonly Color imageMarginGradientEnd;
        readonly Color imageMarginGradientMiddle;
        readonly Color imageMarginRevealedGradientBegin;
        readonly Color imageMarginRevealedGradientEnd;
        readonly Color imageMarginRevealedGradientMiddle;
        readonly Color menuBorder;
        readonly Color menuItemBorder;
        readonly Color menuItemPressedGradientBegin;
        readonly Color menuItemPressedGradientEnd;
        readonly Color menuItemPressedGradientMiddle;
        readonly Color menuItemSelected;
        readonly Color menuItemSelectedGradientBegin;
        readonly Color menuItemSelectedGradientEnd;
        readonly Color menuStripGradientBegin;
        readonly Color menuStripGradientEnd;
        readonly Color overflowButtonGradientBegin;
        readonly Color overflowButtonGradientEnd;
        readonly Color overflowButtonGradientMiddle;
        readonly Color raftingContainerGradientBegin;
        readonly Color raftingContainerGradientEnd;
        readonly Color separatorDark;
        readonly Color separatorLight;
        readonly Color statusStripGradientBegin;
        readonly Color statusStripGradientEnd;
        readonly Color toolStripBorder;
        readonly Color toolStripContentPanelGradientBegin;
        readonly Color toolStripContentPanelGradientEnd;
        readonly Color toolStripDropDownBackground;
        readonly Color toolStripGradientBegin;
        readonly Color toolStripGradientEnd;
        readonly Color toolStripGradientMiddle;
        readonly Color toolStripPanelGradientBegin;
        readonly Color toolStripPanelGradientEnd;

        readonly Color controlBackColor;
        readonly Color controlForeColor;
        readonly Color controlText;
        readonly Color controlDark;
        readonly Color contentPanelBackColor;
        readonly Color inactiveCaption;
        readonly Color windowBackColor;
        readonly Color windowText;

        public MapColorTable(Func<Color, Color> map)
            : this(new ExtendedColorTable(), map)
        {
        }

        public MapColorTable(ExtendedColorTable table, Func<Color, Color> map)
        {
            buttonCheckedGradientBegin = map(table.ButtonCheckedGradientBegin);
            buttonCheckedGradientEnd = map(table.ButtonCheckedGradientEnd);
            buttonCheckedGradientMiddle = map(table.ButtonCheckedGradientMiddle);
            buttonCheckedHighlight = map(table.ButtonCheckedHighlight);
            buttonCheckedHighlightBorder = map(table.ButtonCheckedHighlightBorder);
            buttonPressedBorder = map(table.ButtonPressedBorder);
            buttonPressedGradientBegin = map(table.ButtonPressedGradientBegin);
            buttonPressedGradientEnd = map(table.ButtonPressedGradientEnd);
            buttonPressedGradientMiddle = map(table.ButtonPressedGradientMiddle);
            buttonPressedHighlight = map(table.ButtonPressedHighlight);
            buttonPressedHighlightBorder = map(table.ButtonPressedHighlightBorder);
            buttonSelectedBorder = map(table.ButtonSelectedBorder);
            buttonSelectedGradientBegin = map(table.ButtonSelectedGradientBegin);
            buttonSelectedGradientEnd = map(table.ButtonSelectedGradientEnd);
            buttonSelectedGradientMiddle = map(table.ButtonSelectedGradientMiddle);
            buttonSelectedHighlight = map(table.ButtonSelectedHighlight);
            buttonSelectedHighlightBorder = map(table.ButtonSelectedHighlightBorder);
            checkBackground = map(table.CheckBackground);
            checkPressedBackground = map(table.CheckPressedBackground);
            checkSelectedBackground = map(table.CheckSelectedBackground);
            gripDark = map(table.GripDark);
            gripLight = map(table.GripLight);
            imageMarginGradientBegin = map(table.ImageMarginGradientBegin);
            imageMarginGradientEnd = map(table.ImageMarginGradientEnd);
            imageMarginGradientMiddle = map(table.ImageMarginGradientMiddle);
            imageMarginRevealedGradientBegin = map(table.ImageMarginRevealedGradientBegin);
            imageMarginRevealedGradientEnd = map(table.ImageMarginRevealedGradientEnd);
            imageMarginRevealedGradientMiddle = map(table.ImageMarginRevealedGradientMiddle);
            menuBorder = map(table.MenuBorder);
            menuItemBorder = map(table.MenuItemBorder);
            menuItemPressedGradientBegin = map(table.MenuItemPressedGradientBegin);
            menuItemPressedGradientEnd = map(table.MenuItemPressedGradientEnd);
            menuItemPressedGradientMiddle = map(table.MenuItemPressedGradientMiddle);
            menuItemSelected = map(table.MenuItemSelected);
            menuItemSelectedGradientBegin = map(table.MenuItemSelectedGradientBegin);
            menuItemSelectedGradientEnd = map(table.MenuItemSelectedGradientEnd);
            menuStripGradientBegin = map(table.MenuStripGradientBegin);
            menuStripGradientEnd = map(table.MenuStripGradientEnd);
            overflowButtonGradientBegin = map(table.OverflowButtonGradientBegin);
            overflowButtonGradientEnd = map(table.OverflowButtonGradientEnd);
            overflowButtonGradientMiddle = map(table.OverflowButtonGradientMiddle);
            raftingContainerGradientBegin = map(table.RaftingContainerGradientBegin);
            raftingContainerGradientEnd = map(table.RaftingContainerGradientEnd);
            separatorDark = map(table.SeparatorDark);
            separatorLight = map(table.SeparatorLight);
            statusStripGradientBegin = map(table.StatusStripGradientBegin);
            statusStripGradientEnd = map(table.StatusStripGradientEnd);
            toolStripBorder = map(table.ToolStripBorder);
            toolStripContentPanelGradientBegin = map(table.ToolStripContentPanelGradientBegin);
            toolStripContentPanelGradientEnd = map(table.ToolStripContentPanelGradientEnd);
            toolStripDropDownBackground = map(table.ToolStripDropDownBackground);
            toolStripGradientBegin = map(table.ToolStripGradientBegin);
            toolStripGradientEnd = map(table.ToolStripGradientEnd);
            toolStripGradientMiddle = map(table.ToolStripGradientMiddle);
            toolStripPanelGradientBegin = map(table.ToolStripPanelGradientBegin);
            toolStripPanelGradientEnd = map(table.ToolStripPanelGradientEnd);

            controlBackColor = map(table.ControlBackColor);
            controlForeColor = map(table.ControlForeColor);
            controlText = map(table.ControlText);
            controlDark = map(table.ControlDark);
            contentPanelBackColor = map(table.ContentPanelBackColor);
            inactiveCaption = map(table.InactiveCaption);
            windowBackColor = map(table.WindowBackColor);
            windowText = map(table.WindowText);
        }

        public override Color ButtonCheckedGradientBegin
        {
            get { return buttonCheckedGradientBegin; }
        }

        public override Color ButtonCheckedGradientEnd
        {
            get { return buttonCheckedGradientEnd; }
        }

        public override Color ButtonCheckedGradientMiddle
        {
            get { return buttonCheckedGradientMiddle; }
        }

        public override Color ButtonCheckedHighlight
        {
            get { return buttonCheckedHighlight; }
        }

        public override Color ButtonCheckedHighlightBorder
        {
            get { return buttonCheckedHighlightBorder; }
        }

        public override Color ButtonPressedBorder
        {
            get { return buttonPressedBorder; }
        }

        public override Color ButtonPressedGradientBegin
        {
            get { return buttonPressedGradientBegin; }
        }

        public override Color ButtonPressedGradientEnd
        {
            get { return buttonPressedGradientEnd; }
        }

        public override Color ButtonPressedGradientMiddle
        {
            get { return buttonPressedGradientMiddle; }
        }

        public override Color ButtonPressedHighlight
        {
            get { return buttonPressedHighlight; }
        }

        public override Color ButtonPressedHighlightBorder
        {
            get { return buttonPressedHighlightBorder; }
        }

        public override Color ButtonSelectedBorder
        {
            get { return buttonSelectedBorder; }
        }

        public override Color ButtonSelectedGradientBegin
        {
            get { return buttonSelectedGradientBegin; }
        }

        public override Color ButtonSelectedGradientEnd
        {
            get { return buttonSelectedGradientEnd; }
        }

        public override Color ButtonSelectedGradientMiddle
        {
            get { return buttonSelectedGradientMiddle; }
        }

        public override Color ButtonSelectedHighlight
        {
            get { return buttonSelectedHighlight; }
        }

        public override Color ButtonSelectedHighlightBorder
        {
            get { return buttonSelectedHighlightBorder; }
        }

        public override Color CheckBackground
        {
            get { return checkBackground; }
        }

        public override Color CheckPressedBackground
        {
            get { return checkPressedBackground; }
        }

        public override Color CheckSelectedBackground
        {
            get { return checkSelectedBackground; }
        }

        public override Color GripDark
        {
            get { return gripDark; }
        }

        public override Color GripLight
        {
            get { return gripLight; }
        }

        public override Color ImageMarginGradientBegin
        {
            get { return imageMarginGradientBegin; }
        }

        public override Color ImageMarginGradientEnd
        {
            get { return imageMarginGradientEnd; }
        }

        public override Color ImageMarginGradientMiddle
        {
            get { return imageMarginGradientMiddle; }
        }

        public override Color ImageMarginRevealedGradientBegin
        {
            get { return imageMarginRevealedGradientBegin; }
        }

        public override Color ImageMarginRevealedGradientEnd
        {
            get { return imageMarginRevealedGradientEnd; }
        }

        public override Color ImageMarginRevealedGradientMiddle
        {
            get { return imageMarginRevealedGradientMiddle; }
        }

        public override Color MenuBorder
        {
            get { return menuBorder; }
        }

        public override Color MenuItemBorder
        {
            get { return menuItemBorder; }
        }

        public override Color MenuItemPressedGradientBegin
        {
            get { return menuItemPressedGradientBegin; }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get { return menuItemPressedGradientEnd; }
        }

        public override Color MenuItemPressedGradientMiddle
        {
            get { return menuItemPressedGradientMiddle; }
        }

        public override Color MenuItemSelected
        {
            get { return menuItemSelected; }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return menuItemSelectedGradientBegin; }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return menuItemSelectedGradientEnd; }
        }

        public override Color MenuStripGradientBegin
        {
            get { return menuStripGradientBegin; }
        }

        public override Color MenuStripGradientEnd
        {
            get { return menuStripGradientEnd; }
        }

        public override Color OverflowButtonGradientBegin
        {
            get { return overflowButtonGradientBegin; }
        }

        public override Color OverflowButtonGradientEnd
        {
            get { return overflowButtonGradientEnd; }
        }

        public override Color OverflowButtonGradientMiddle
        {
            get { return overflowButtonGradientMiddle; }
        }

        public override Color RaftingContainerGradientBegin
        {
            get { return raftingContainerGradientBegin; }
        }

        public override Color RaftingContainerGradientEnd
        {
            get { return raftingContainerGradientEnd; }
        }

        public override Color SeparatorDark
        {
            get { return separatorDark; }
        }

        public override Color SeparatorLight
        {
            get { return separatorLight; }
        }

        public override Color StatusStripGradientBegin
        {
            get { return statusStripGradientBegin; }
        }

        public override Color StatusStripGradientEnd
        {
            get { return statusStripGradientEnd; }
        }

        public override Color ToolStripBorder
        {
            get { return toolStripBorder; }
        }

        public override Color ToolStripContentPanelGradientBegin
        {
            get { return toolStripContentPanelGradientBegin; }
        }

        public override Color ToolStripContentPanelGradientEnd
        {
            get { return toolStripContentPanelGradientEnd; }
        }

        public override Color ToolStripDropDownBackground
        {
            get { return toolStripDropDownBackground; }
        }

        public override Color ToolStripGradientBegin
        {
            get { return toolStripGradientBegin; }
        }

        public override Color ToolStripGradientEnd
        {
            get { return toolStripGradientEnd; }
        }

        public override Color ToolStripGradientMiddle
        {
            get { return toolStripGradientMiddle; }
        }

        public override Color ToolStripPanelGradientBegin
        {
            get { return toolStripPanelGradientBegin; }
        }

        public override Color ToolStripPanelGradientEnd
        {
            get { return toolStripPanelGradientEnd; }
        }

        public override Color ControlBackColor
        {
            get { return controlBackColor; }
        }

        public override Color ControlForeColor
        {
            get { return controlForeColor; }
        }

        public override Color ControlText
        {
            get { return controlText; }
        }

        public override Color ControlDark
        {
            get { return controlDark; }
        }

        public override Color ContentPanelBackColor
        {
            get { return contentPanelBackColor; }
        }

        public override Color InactiveCaption
        {
            get { return inactiveCaption; }
        }

        public override Color WindowBackColor
        {
            get { return windowBackColor; }
        }

        public override Color WindowText
        {
            get { return windowText; }
        }
    }
}
