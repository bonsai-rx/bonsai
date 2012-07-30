using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Drawing;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Reactive.Subjects;

namespace Bonsai.IO
{
    public class Mouse : Source<Point>
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        class MouseMessageFilter : IMessageFilter, IDisposable
        {
            const int WM_MOUSEMOVE = 0x200;
            readonly Subject<Point> mouseMove;

            public MouseMessageFilter()
            {
                mouseMove = new Subject<Point>();
                Application.AddMessageFilter(this);
            }

            public IObservable<Point> MouseMove
            {
                get { return mouseMove; }
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    mouseMove.OnNext(Form.MousePosition);
                }

                return false;
            }

            public void Dispose()
            {
                Application.RemoveMessageFilter(this);
            }
        }

        protected override IObservable<Point> Generate()
        {
            return Observable.Using(
                () => new MouseMessageFilter(),
                filter => filter.MouseMove);
        }
    }
}
