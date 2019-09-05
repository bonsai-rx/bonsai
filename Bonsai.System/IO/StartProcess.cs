using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [DefaultProperty("FileName")]
    [Description("Starts a new system process with the specified file name and optional command-line arguments.")]
    public class StartProcess : Source<int>
    {
        [Description("The name of the application to start.")]
        [FileNameFilter("Executable files|*.exe|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        [Description("The optional set of command-line arguments to use when starting the application.")]
        public string Arguments { get; set; }

        public IObservable<int> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(input => Generate());
        }

        public override IObservable<int> Generate()
        {
            return Observable.StartAsync(cancellationToken =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using (var exitSignal = new ManualResetEvent(false))
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = FileName;
                        process.StartInfo.Arguments = Arguments;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.ErrorDataReceived += (sender, e) => Console.Error.WriteLine(e.Data);
                        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                        process.Exited += (sender, e) => exitSignal.Set();
                        process.EnableRaisingEvents = true;
                        process.Start();
                        process.BeginErrorReadLine();
                        process.BeginOutputReadLine();
                        using (var cancellation = cancellationToken.Register(() => exitSignal.Set()))
                        {
                            exitSignal.WaitOne();
                            if (!process.HasExited) return 0;
                            return process.ExitCode;
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
