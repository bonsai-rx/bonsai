using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    class InterceptKeys
    {
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYUP = 0x105;
        LowLevelKeyboardProc proc;
        ApplicationContext hookContext;
        Subject<Keys> keyDown;
        Subject<Keys> keyUp;
        IntPtr hookId;
        int hookCount;
        Task hookTask;
        object gate;

        private InterceptKeys()
        {
            hookTask = Task.FromResult(IntPtr.Zero);
            keyDown = new Subject<Keys>();
            keyUp = new Subject<Keys>();
            gate = new object();

            KeyDown = Observable.Using(
                () => RegisterHook(),
                resource => keyDown)
                .PublishReconnectable()
                .RefCount();

            KeyUp = Observable.Using(
                () => RegisterHook(),
                resource => keyUp)
                .PublishReconnectable()
                .RefCount();
        }

        static readonly Lazy<InterceptKeys> instance = new Lazy<InterceptKeys>(() => new InterceptKeys());

        public static InterceptKeys Instance
        {
            get { return instance.Value; }
        }

        public IObservable<Keys> KeyDown { get; private set; }

        public IObservable<Keys> KeyUp { get; private set; }

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

        private ApplicationContext SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                var threadContext = new ApplicationContext();
                hookTask = hookTask.ContinueWith(previous =>
                {
                    hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                    Application.Run(threadContext);
                    UnhookWindowsHookEx(hookId);
                    hookId = IntPtr.Zero;
                }, TaskContinuationOptions.LongRunning);
                return threadContext;
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                keyDown.OnNext((Keys)vkCode);
            }
            else if (nCode >= 0 && (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                keyUp.OnNext((Keys)vkCode);
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
