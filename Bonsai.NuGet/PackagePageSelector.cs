using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Bonsai.NuGet
{
    partial class PackagePageSelector : UserControl
    {
        const int MaxPageButtons = 5;
        Button[] buttons;
        int selectedIndex;
        int indexOffset;
        int pageCount;
        Font normalFont;
        Font boldFont;

        public PackagePageSelector()
        {
            InitializeComponent();
            buttons = new[] { button1, button2, button3, button4, button5 };
            for (int i = 0; i < buttons.Length; i++)
            {
                var buttonIndex = i;
                var button = buttons[i];
                button.Visible = false;
                button.Click += delegate { SelectedIndex = indexOffset + buttonIndex; };
                buttons[i] = button;
            }
            previousButton.Visible = false;
            nextButton.Visible = false;

            nextButton.Click += nextButton_Click;
            previousButton.Click += previousButton_Click;
            normalFont = nextButton.Font;
            boldFont = new Font(normalFont, FontStyle.Bold);
        }

        [Category("Behavior")]
        public event EventHandler SelectedIndexChanged;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = pageCount > 0 ? value : -1;
                RefreshLayout();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        public int PageCount
        {
            get { return pageCount; }
            set
            {
                pageCount = value;
                for (int i = 0; i < MaxPageButtons; i++)
                {
                    var button = buttons[i];
                    button.Visible = i < pageCount;
                }

                RefreshLayout();
            }
        }

        private void OnSelectedIndexChanged(EventArgs e)
        {
            var handler = SelectedIndexChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void previousButton_Click(object sender, EventArgs e)
        {
            var index = Math.Max(selectedIndex - 1, 0);
            if (index < indexOffset) indexOffset--;
            SelectedIndex = index;
        }

        void nextButton_Click(object sender, EventArgs e)
        {
            var index = Math.Min(selectedIndex + 1, pageCount - 1);
            if (index >= indexOffset + MaxPageButtons) indexOffset++;
            SelectedIndex = index;
        }

        void RefreshLayout()
        {
            SuspendLayout();
            previousButton.Visible = false;
            nextButton.Visible = false;
            if (pageCount > 1)
            {
                if (selectedIndex > 0) previousButton.Visible = true;
                if (selectedIndex < pageCount - 1) nextButton.Visible = true;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                var buttonIndex = indexOffset + i;
                button.Font = buttonIndex == selectedIndex ? boldFont : normalFont;
                button.Text = (buttonIndex + 1).ToString();
            }
            ResumeLayout();
        }
    }
}
