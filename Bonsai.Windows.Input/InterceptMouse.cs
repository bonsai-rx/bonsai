using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    class InterceptMouse
    {
        const int WH_MOUSE_LL = 14;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_MOUSEWHEEL = 0x020A;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_MBUTTONUP = 0x0208;
        LowLevelMouseProc proc;
        ApplicationContext hookContext;
        Subject<Point> mouseMove;
        Subject<int> mouseWheel;
        Subject<MouseButtons> mouseButtonDown;
        Subject<MouseButtons> mouseButtonUp;
        IntPtr hookId;
        int hookCount;
        Task hookTask;
        object gate;

        private InterceptMouse()
        {
            hookTask = Task.FromResult(IntPtr.Zero);
            mouseMove = new Subject<Point>();
            mouseWheel = new Subject<int>();
            mouseButtonDown = new Subject<MouseButtons>();
            mouseButtonUp = new Subject<MouseButtons>();
            gate = new object();

            MouseMove = Observable.Using(
                () => RegisterHook(),
                resource => mouseMove)
                .PublishReconnectable()
                .RefCount();

            MouseWheel = Observable.Using(
                () => RegisterHook(),
                resource => mouseWheel)
                .PublishReconnectable()
                .RefCount();

            MouseButtonDown = Observable.Using(
                () => RegisterHook(),
                resource => mouseButtonDown)
                .PublishReconnectable()
                .RefCount();

            MouseButtonUp = Observable.Using(
                () => RegisterHook(),
                resource => mouseButtonUp)
                .PublishReconnectable()
                .RefCount();
        }

        static readonly Lazy<InterceptMouse> instance = new Lazy<InterceptMouse>(() => new InterceptMouse());

        public static InterceptMouse Instance
        {
            get { return instance.Value; }
        }

        public IObservable<Point> MouseMove { get; private set; }

        public IObservable<int> MouseWheel { get; private set; }

        public IObservable<MouseButtons> MouseButtonDown { get; private set; }

        public IObservable<MouseButtons> MouseButtonUp { get; private set; }

        private IDisposable RegisterHook()
        {
            lock (gate)
            {
                if (hookContext == null)
                {
                    proc = HookCallback;
                    hookContext = SetHook(proc);
                }

                hookCount++;
            }

            return Disposable.Create(() =>
            {
                lock (gate)
                {
                    if (--hookCount <= 0)
                    {
                        hookContext.ExitThread();
                        hookContext = null;
                        hookCount = 0;
                        proc = null;
                    }
                }
            });
        }

        private ApplicationContext SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                var threadContext = new ApplicationContext();
                hookTask = hookTask.ContinueWith(previous =>
                {
                    hookId = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                    Application.Run(threadContext);
                    UnhookWindowsHookEx(hookId);
                    hookId = IntPtr.Zero;
                }, TaskContinuationOptions.LongRunning);
                return threadContext;
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_MOUSEMOVE)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    mouseMove.OnNext(new Point(hookStruct.pt.x, hookStruct.pt.y));
                }
                else if (wParam == (IntPtr)WM_MOUSEWHEEL)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    mouseWheel.OnNext((int)hookStruct.mouseData >> 16);
                }
                else if (wParam == (IntPtr)WM_LBUTTONDOWN) mouseButtonDown.OnNext(MouseButtons.Left);
                else if (wParam == (IntPtr)WM_LBUTTONUP) mouseButtonUp.OnNext(MouseButtons.Left);
                else if (wParam == (IntPtr)WM_RBUTTONDOWN) mouseButtonDown.OnNext(MouseButtons.Right);
                else if (wParam == (IntPtr)WM_RBUTTONUP) mouseButtonUp.OnNext(MouseButtons.Right);
                else if (wParam == (IntPtr)WM_MBUTTONDOWN) mouseButtonDown.OnNext(MouseButtons.Middle);
                else if (wParam == (IntPtr)WM_MBUTTONUP) mouseButtonUp.OnNext(MouseButtons.Middle);
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
