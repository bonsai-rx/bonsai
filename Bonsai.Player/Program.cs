using System;
using System.IO;
using System.Xml;
using Bonsai.Expressions;
using System.Threading;
using System.Reactive.Linq;
using Bonsai.Configuration;
using System.Collections.Generic;

namespace Bonsai.Player
{
    class Program
    {
        const string PropertyCommand = "--property";

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: Bonsai.Player <workflowFileName> [options]");
                return;
            }

            var fileName = default(string);
            var propertyAssignments = new Dictionary<string, string>();
            var parser = new CommandLineParser();
            parser.RegisterCommand(command => fileName = command);
            parser.RegisterCommand(PropertyCommand, property =>
            {
                var assignment = PropertyAssignment.Parse(property);
                propertyAssignments.Add(assignment.Name, assignment.Value);
            });
            parser.Parse(args);

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.");
            }

            WorkflowBuilder workflowBuilder;
            var packageConfiguration = ConfigurationHelper.Load();
            Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, AppDomain.CurrentDomain.BaseDirectory);
            ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
            using (var reader = XmlReader.Create(fileName))
            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }

            workflowBuilder.Workflow.Build();
            foreach (var assignment in propertyAssignments)
            {
                workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
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
