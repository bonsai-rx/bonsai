using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that starts a new system process with the specified file name and
    /// command-line arguments.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Starts a new system process with the specified file name and command-line arguments.")]
    public class StartProcess : Source<int>
    {
        /// <summary>
        /// Gets or sets the name of the application or document to start.
        /// </summary>
        [Description("The name of the application or document to start.")]
        [FileNameFilter("Executable files|*.exe|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the set of command-line arguments to use when starting the application.
        /// </summary>
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        [Description("The set of command-line arguments to use when starting the application.")]
        public string Arguments { get; set; }

        /// <summary>
        /// Starts a new system process with the specified file name and command-line arguments
        /// for each element of an observable sequence, and surfaces all the exit codes as the
        /// processes terminate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// An observable sequence used to start the system processes. For each element produced by
        /// this sequence, a new system process will be started.
        /// </param>
        /// <returns>
        /// A sequence containing all the exit codes from the processes started by the
        /// <paramref name="source"/> sequence. A new exit code is produced every time one
        /// of the processes terminates.
        /// </returns>
        public IObservable<int> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(input => Generate());
        }

        /// <summary>
        /// Starts a new system process with the specified file name and command-line arguments
        /// and surfaces the exit code when the process terminates through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing the exit code that the process specified when it terminated.
        /// </returns>
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
