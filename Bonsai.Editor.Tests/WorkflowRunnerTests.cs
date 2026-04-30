using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class WorkflowRunnerTests
    {
        [TestMethod]
        [DataRow("IncludeWorkflowBuildException.bonsai", "NestedWorkflowBuildException.bonsai")]
        [DataRow("IncludeWorkflowRuntimeException.bonsai", "NestedWorkflowRuntimeException.bonsai")]
        [DataRow("IncludeEmbeddedWorkflowBuildException.bonsai", "Bonsai.Editor.Tests:NestedWorkflowBuildException.bonsai")]
        [DataRow("IncludeEmbeddedWorkflowRuntimeException.bonsai", "Bonsai.Editor.Tests:NestedWorkflowRuntimeException.bonsai")]
        public void Run_NestedWorkflowException_ErrorMessageIncludesLineNumbers(string fileName, string nestedPath)
        {
            var output = RunAndCaptureError(fileName);
            StringAssert.Matches(output, new Regex($@"at .+ in {Regex.Escape(nestedPath)}:line \d+"));
            StringAssert.Matches(output, new Regex($@"at .+ in {Regex.Escape(fileName)}:line \d+"));
        }

        static string RunAndCaptureError(string fileName)
        {
            var originalError = Console.Error;
            var writer = new StringWriter();
            Console.SetError(writer);
            try
            {
                WorkflowRunner.Run(fileName, new(), visualizerProvider: null);
            }
            finally
            {
                Console.SetError(originalError);
            }
            return writer.ToString();
        }
    }
}
