using System.Xml;
using Bonsai.Expressions;
using System.Reactive.Linq;
using Bonsai.Configuration;
using System.Threading.Tasks;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace Bonsai.Player
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var filePath = new Argument<FileInfo>(
                name: "WORKFLOW",
                description: "The workflow file to run.",
                parse: result =>
                {
                    var filePath = result.Tokens.Single().Value;
                    if (!File.Exists(filePath))
                    {
                        result.ErrorMessage = "The specified workflow file path does not exist.";
                        return null;
                    }
                    else return new FileInfo(filePath);
                });
            var rootCommand = new RootCommand("Run Bonsai workflows from the command-line.");
            rootCommand.AddArgument(filePath);
            rootCommand.SetHandler(async filePath =>
            {
                WorkflowBuilder workflowBuilder;
                ConfigurationHelper.SetAssemblyResolve();
                using (var reader = XmlReader.Create(filePath.FullName))
                {
                    workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
                }

                await workflowBuilder.Workflow.BuildObservable();
            }, filePath);
            await rootCommand.InvokeAsync(args);
        }
    }
}
