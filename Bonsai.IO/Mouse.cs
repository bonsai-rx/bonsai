using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Drawing;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace Bonsai.IO
{
    public class Mouse : Source<Point>
    {
        MouseMessageFilter messageFilter;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public class MouseMessageFilter : IMessageFilter
        {
            const int WM_MOUSEMOVE = 0x200;
            Mouse source;

            public MouseMessageFilter(Mouse mouseSource)
            {
                source = mouseSource;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    source.Subject.OnNext(Form.MousePosition);
                }

                return false;
            }
        }

        public override IDisposable Load()
        {
            messageFilter = new MouseMessageFilter(this);
            return base.Load();
        }

        protected override void Unload()
        {
            messageFilter = null;
            base.Unload();
        }

        protected override void Start()
        {
            Application.AddMessageFilter(messageFilter);
        }

        protected override void Stop()
        {
            Application.RemoveMessageFilter(messageFilter);
        }
    }
}
