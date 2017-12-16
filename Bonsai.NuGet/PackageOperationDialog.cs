using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            logger.Log += logger_Log;
            eventLogger = logger;
        }

        private void ClearEventLogger()
        {
            if (eventLogger != null)
            {
                eventLogger.Log -= logger_Log;
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
                if (e.Level == MessageLevel.Error)
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
            var message = string.Format(item.Message, item.Args);

            Brush itemBrush;
            switch (item.Level)
            {
                case MessageLevel.Debug: itemBrush = Brushes.Yellow; break;
                case MessageLevel.Error: itemBrush = Brushes.Red; break;
                case MessageLevel.Warning: itemBrush = Brushes.Violet; break;
                case MessageLevel.Info:
                default: itemBrush = Brushes.Black; break;
            }

            e.Graphics.DrawString(message, e.Font, itemBrush, e.Bounds);
        }

        private void loggerListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0) return;

            var item = (LogEventArgs)loggerListBox.Items[e.Index];
            var message = string.Format(item.Message, item.Args);

            var size = e.Graphics.MeasureString(message, loggerListBox.Font, loggerListBox.Width);
            e.ItemWidth = (int)size.Width;
            e.ItemHeight = (int)size.Height;
        }
    }
}
