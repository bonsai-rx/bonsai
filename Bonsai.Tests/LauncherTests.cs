using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Tests;

[TestClass]
[DoNotParallelize]
public sealed class LauncherTests
{
    const string InvalidXml = "not xml";

    const string ExternalizedTimerWorkflow =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <WorkflowBuilder Version="2.9.0"
                         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                         xmlns:rx="clr-namespace:Bonsai.Reactive;assembly=Bonsai.Core"
                         xmlns="https://bonsai-rx.org/2018/workflow">
          <Workflow>
            <Nodes>
              <Expression xsi:type="ExternalizedMapping">
                <Property Name="DueTime" />
              </Expression>
              <Expression xsi:type="Combinator">
                <Combinator xsi:type="rx:Timer">
                  <rx:DueTime>PT0S</rx:DueTime>
                  <rx:Period>PT0S</rx:Period>
                </Combinator>
              </Expression>
            </Nodes>
            <Edges>
              <Edge From="0" To="1" Label="Source1" />
            </Edges>
          </Workflow>
        </WorkflowBuilder>
        """;

    [TestMethod]
    public void LaunchWorkflowCheck_NoWorkflowFile_ReturnsError()
    {
        var output = CaptureError(() => Launcher.LaunchWorkflowCheck(null, new()), out var exitCode);
        Assert.AreEqual(Program.ErrorExitCode, exitCode);
        StringAssert.Contains(output, Properties.Resources.WorkflowFileNotSpecifiedWarning);
    }

    [TestMethod]
    public void LaunchWorkflowCheck_ValidPropertyValue_ReturnsNormalExit()
    {
        var output = CheckWorkflow(ExternalizedTimerWorkflow, out var exitCode, ("DueTime", "00:00:01"));
        Assert.AreEqual(Program.NormalExitCode, exitCode);
        Assert.AreEqual(string.Empty, output);
    }

    [TestMethod]
    public void LaunchWorkflowCheck_InvalidPropertyValue_ReportsValueAndType()
    {
        var output = CheckWorkflow(ExternalizedTimerWorkflow, out var exitCode, ("DueTime", "abc"));
        Assert.AreEqual(Program.ErrorExitCode, exitCode);
        StringAssert.Contains(output, "abc is not a valid value for TimeSpan");
    }

    [TestMethod]
    public void LaunchWorkflowCheck_InvalidXml_ReturnsError()
    {
        var output = CheckWorkflow(InvalidXml, out var exitCode);
        Assert.AreEqual(Program.ErrorExitCode, exitCode);
        StringAssert.Contains(output, "is not a valid workflow");
    }

    static string CheckWorkflow(string workflow, out int exitCode, params (string Name, string Value)[] properties)
    {
        var propertyAssignments = new Dictionary<string, string>();
        foreach (var (name, value) in properties)
            propertyAssignments.Add(name, value);

        return WithWorkflowFile(workflow, out exitCode, path => Launcher.LaunchWorkflowCheck(path, propertyAssignments));
    }

    static string WithWorkflowFile(string workflow, out int exitCode, Func<string, int> launch)
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bonsai");
        File.WriteAllText(path, workflow);
        try { return CaptureError(() => launch(path), out exitCode); }
        finally { File.Delete(path); }
    }

    static string CaptureError(Func<int> launch, out int exitCode)
    {
        var originalError = Console.Error;
        var writer = new StringWriter();
        Console.SetError(writer);
        try
        {
            exitCode = launch();
        }
        finally
        {
            Console.SetError(originalError);
        }
        return writer.ToString();
    }
}
