using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class CueBannerTextBox : TextBox
    {
        Color activeForeColor;

        public string CueBanner { get; set; }

        public bool CueBannerVisible { get; private set; }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A: SelectAll(); break;
                    case Keys.C: Copy(); break;
                    case Keys.V: Paste(); break;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private void ShowCueBanner()
        {
            if (string.IsNullOrWhiteSpace(Text) && !string.IsNullOrWhiteSpace(CueBanner))
            {
                CueBannerVisible = true;
                activeForeColor = ForeColor;
                ForeColor = Color.Gray;
                Text = CueBanner;
            }
        }

        private void HideCueBanner()
        {
            if (CueBannerVisible)
            {
                CueBannerVisible = false;
                Text = string.Empty;
                ForeColor = activeForeColor;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!Focused) ShowCueBanner();
            base.OnHandleCreated(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            HideCueBanner();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            ShowCueBanner();
            base.OnLeave(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (!CueBannerVisible)
            {
                base.OnTextChanged(e);
            }
        }
    }
}
