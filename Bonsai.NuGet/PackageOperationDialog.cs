using Bonsai.NuGet.Properties;
using NuGet.Common;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageOperationDialog : Form
    {
        EventLogger eventLogger;

        public PackageOperationDialog()
        {
            InitializeComponent();
            Text = Resources.InstallOperationLabel;
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                SetOperationLabel(value);
            }
        }

        private void SetOperationLabel(string text)
        {
            actionNameLabel.Text = text;
            using (var graphics = CreateGraphics())
            {
                var textSize = graphics.MeasureString(text, actionNameLabel.Font);
                actionNameLabel.Location = new Point((actionNamePanel.Width - (int)textSize.Width) / 2, actionNameLabel.Location.Y);
            }
        }

        public void RegisterEventLogger(EventLogger logger)
        {
            ClearEventLogger();
            logger.LogMessage += logger_Log;
            eventLogger = logger;
        }

        private void ClearEventLogger()
        {
            if (eventLogger != null)
            {
                eventLogger.LogMessage -= logger_Log;
                eventLogger = null;
            }
        }

        public void Complete()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void logger_Log(object sender, LogEventArgs e)
        {
            if (InvokeRequired) BeginInvoke((EventHandler<LogEventArgs>)logger_Log, sender, e);
            else
            {
                loggerListBox.Items.Add(e);
                loggerListBox.TopIndex = loggerListBox.Items.Count - 1;
                if (e.Message.Level == LogLevel.Error)
                {
                    progressBar.ForeColor = Color.Red;
                    progressBar.Style = ProgressBarStyle.Blocks;
                    SetOperationLabel(Resources.FailedOperationLabel);
                }
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loggerListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            e.DrawFocusRectangle();

            var item = (LogEventArgs)loggerListBox.Items[e.Index];
            Brush itemBrush;
            switch (item.Message.Level)
            {
                case LogLevel.Debug: itemBrush = Brushes.Yellow; break;
                case LogLevel.Error: itemBrush = Brushes.Red; break;
                case LogLevel.Warning: itemBrush = Brushes.Violet; break;
                case LogLevel.Information:
                default: itemBrush = Brushes.Black; break;
            }

            e.Graphics.DrawString(item.Message.Message, e.Font, itemBrush, e.Bounds);
        }

        private void loggerListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0) return;

            var item = (LogEventArgs)loggerListBox.Items[e.Index];
            var size = e.Graphics.MeasureString(item.Message.Message, loggerListBox.Font, loggerListBox.Width);
            e.ItemWidth = (int)size.Width;
            e.ItemHeight = (int)size.Height;
        }
    }
}
