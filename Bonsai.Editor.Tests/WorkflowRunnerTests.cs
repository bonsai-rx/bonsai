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
        const string MissingTypesWorkflow = """
            <?xml version="1.0" encoding="utf-8"?>
            <WorkflowBuilder Version="2.8.0"
                             xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                             xmlns:rx="clr-namespace:Bonsai.Reactive;assembly=Bonsai.Core"
                             xmlns="https://bonsai-rx.org/2018/workflow">
              <Workflow>
                <Nodes>
                  <Expression xsi:type="Combinator">
                    <Combinator xsi:type="rx:UnknownDummyType" />
                  </Expression>
                </Nodes>
                <Edges />
              </Workflow>
            </WorkflowBuilder>
            """;

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

        [TestMethod]
        [DataRow("IncludeWorkflowBuildException.bonsai", "NestedWorkflowBuildException.bonsai")]
        [DataRow("IncludeEmbeddedWorkflowBuildException.bonsai", "Bonsai.Editor.Tests:NestedWorkflowBuildException.bonsai")]
        public void Check_NestedWorkflowBuildException_FailsWithLineNumbers(string fileName, string nestedPath)
        {
            var output = CaptureError(() => Assert.IsFalse(WorkflowRunner.Check(fileName, new())));
            StringAssert.Matches(output, new Regex($@"at .+ in {Regex.Escape(nestedPath)}:line \d+"));
            StringAssert.Matches(output, new Regex($@"at .+ in {Regex.Escape(fileName)}:line \d+"));
        }

        [TestMethod]
        [DataRow("IncludeWorkflowRuntimeException.bonsai")]
        [DataRow("IncludeEmbeddedWorkflowRuntimeException.bonsai")]
        public void Check_NestedWorkflowRuntimeException_SucceedsWithoutRunning(string fileName)
        {
            var output = CaptureError(() => Assert.IsTrue(WorkflowRunner.Check(fileName, new())));
            Assert.AreEqual(string.Empty, output);
        }

        [TestMethod]
        public void Check_MissingTypes_Fails()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bonsai");
            File.WriteAllText(path, MissingTypesWorkflow);
            try
            {
                var output = CaptureError(() => Assert.IsFalse(WorkflowRunner.Check(path, new())));
                StringAssert.Contains(output, "UnknownDummyType");
            }
            finally { File.Delete(path); }
        }

        static string RunAndCaptureError(string fileName)
        {
            return CaptureError(() => WorkflowRunner.Run(fileName, new(), visualizerProvider: null));
        }

        static string CaptureError(Action action)
        {
            var originalError = Console.Error;
            var writer = new StringWriter();
            Console.SetError(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetError(originalError);
            }
            return writer.ToString();
        }
    }
}
