using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    partial class PackagePageSelector : UserControl
    {
        int selectedPage;
        Font boldFont;

        public PackagePageSelector()
        {
            InitializeComponent();
            previousButton.Visible = false;
            nextButton.Visible = false;

            nextButton.Click += nextButton_Click;
            previousButton.Click += previousButton_Click;
            boldFont = new Font(nextButton.Font, FontStyle.Bold);
            currentButton.Font = boldFont;
            RefreshLayout();
        }

        [Category("Behavior")]
        public event EventHandler SelectedIndexChanged;

        public int SelectedPage
        {
            get { return selectedPage; }
            set
            {
                selectedPage = value;
                RefreshLayout();
                OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        public bool ShowNext
        {
            get { return nextButton.Visible; }
            set { nextButton.Visible = value; }
        }

        private void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            RefreshLayout();
            base.OnVisibleChanged(e);
        }

        void previousButton_Click(object sender, EventArgs e)
        {
            var index = Math.Max(selectedPage - 1, 0);
            if (index < selectedPage) selectedPage--;
            SelectedPage = index;
        }

        void nextButton_Click(object sender, EventArgs e)
        {
            SelectedPage++;
        }

        void RefreshLayout()
        {
            SuspendLayout();
            previousButton.Visible = selectedPage > 0;
            currentButton.Text = (selectedPage + 1).ToString();
            ResumeLayout();
        }
    }
}
