using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Expressions;
using System.Threading;

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
            using (var reader = XmlReader.Create(fileName))
            {
                var serializer = new XmlSerializer(typeof(WorkflowBuilder));
                workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
            }

            var workflowCompleted = new ManualResetEvent(false);
            var runningWorkflow = workflowBuilder.Workflow.Build();
            var subscribeExpression = runningWorkflow.BuildSubscribe(ex => { Console.WriteLine(ex); workflowCompleted.Set(); }, () => workflowCompleted.Set());
            var subscriber = subscribeExpression.Compile();

            using (var loaded = runningWorkflow.Load())
            using (var running = subscriber())
            {
                workflowCompleted.WaitOne();
            }
        }
    }
}
