using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Expressions;
using System.Threading;
using System.Reactive.Linq;
using Bonsai.Configuration;

namespace Bonsai.Player
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: Bonsai.Player <workflowFileName>");
                return;
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.");
            }

            WorkflowBuilder workflowBuilder;
            var packageConfiguration = ConfigurationHelper.Load();
            Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, Environment.CurrentDirectory);
            ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
            using (var reader = XmlReader.Create(fileName))
            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }

            var workflowCompleted = new ManualResetEvent(false);
            workflowBuilder.Workflow.BuildObservable().Subscribe(
                unit => { },
                ex => { Console.WriteLine(ex); workflowCompleted.Set(); },
                () => workflowCompleted.Set());
            workflowCompleted.WaitOne();
        }
    }
}
