using System;
using System.Windows.Forms;
using BonsaiLauncher.Properties;

namespace BonsaiLauncher
{
    internal static class CommonDialog
    {
        public static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, Resources.BonsaiLauncher_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
