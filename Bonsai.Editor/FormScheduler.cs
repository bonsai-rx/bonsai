using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class FormScheduler : IScheduler
    {
        bool formClosed;
        readonly object formClosedGate = new object();

        public FormScheduler(Form form)
        {
            Form = form;
            Form.FormClosed += Form_FormClosed;
        }

        public Form Form { get; private set; }

        void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            lock (formClosedGate)
            {
                formClosed = true;
            }
        }

        public DateTimeOffset Now
        {
            get { return DateTimeOffset.Now; }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return Disposable.Empty;
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return Disposable.Empty;
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var result = new SingleAssignmentDisposable();
            lock (formClosedGate)
            {
                if (!formClosed)
                {
                    Form.BeginInvoke((Action)(() =>
                    {
                        if (!result.IsDisposed)
                        {
                            result.Disposable = action(this, state);
                        }
                    }));
                }
            }

            return result;
        }
    }
}
