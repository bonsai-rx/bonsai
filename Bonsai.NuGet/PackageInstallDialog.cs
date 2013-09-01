using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageInstallDialog : Form
    {
        IDisposable logHandler;

        public PackageInstallDialog()
        {
            InitializeComponent();
        }

        internal void RegisterEventLogger(EventLogger logger)
        {
            logHandler = Observable.FromEventPattern<LogEventArgs>(
                handler => logger.Log += new EventHandler<LogEventArgs>(handler),
                handler => logger.Log -= new EventHandler<LogEventArgs>(handler))
                .ObserveOn(this)
                .Subscribe(evt =>
                {
                    var eventArgs = evt.EventArgs;
                    var message = string.Format(eventArgs.Message, eventArgs.Args);
                    loggerListBox.Items.Add(message);
                });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (logHandler != null)
            {
                logHandler.Dispose();
                logHandler = null;
            }
            base.OnFormClosed(e);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
