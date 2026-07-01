using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Configuration.Tests;

[TestClass]
public sealed class CommandLineParserTests
{
    const string PropertyCommand = "--property";

    static CommandLineParser CreateParser(out List<string> values)
    {
        var captured = new List<string>();
        values = captured;
        var parser = new CommandLineParser();
        parser.RegisterCommand(PropertyCommand, captured.Add);
        return parser;
    }

    [TestMethod]
    public void GetArgumentIndices_InlineValue_ReturnsOptionIndex()
    {
        var parser = CreateParser(out var values);
        parser.Parse(["--property:Foo=Bar"]);
        CollectionAssert.AreEqual((int[])[0], parser.GetArgumentIndices(PropertyCommand).ToArray());
        CollectionAssert.AreEqual((string[])["Foo=Bar"], values.ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_SeparateValueToken_ReturnsOptionAndValueIndices()
    {
        var parser = CreateParser(out var values);
        parser.Parse(["--property", "Foo=Bar"]);
        CollectionAssert.AreEqual((int[])[0, 1], parser.GetArgumentIndices(PropertyCommand).ToArray());
        CollectionAssert.AreEqual((string[])["Foo=Bar"], values.ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_ShorthandAlias_ResolvesToSameCommand()
    {
        var parser = CreateParser(out var values);
        parser.Parse(["-p:Foo=Bar"]);
        CollectionAssert.AreEqual((int[])[0], parser.GetArgumentIndices(PropertyCommand).ToArray());
        CollectionAssert.AreEqual((string[])["Foo=Bar"], values.ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_QueriedByAlias_ReturnsSameIndices()
    {
        var parser = CreateParser(out _);
        parser.Parse(["--property:Foo=Bar"]);
        CollectionAssert.AreEqual((int[])[0], parser.GetArgumentIndices("-p").ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_RepeatedOccurrences_AccumulatesIndices()
    {
        var parser = CreateParser(out var values);
        parser.RegisterCommand("--start", () => { });
        parser.Parse(["--property:A=1", "--start", "--property", "B=2"]);
        CollectionAssert.AreEqual((int[])[0, 2, 3], parser.GetArgumentIndices(PropertyCommand).ToArray());
        CollectionAssert.AreEqual((string[])["A=1", "B=2"], values.ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_InterleavedWithOtherArguments_ReturnsOnlyOwnTokens()
    {
        var parser = CreateParser(out _);
        parser.RegisterCommand("--editor-scale", scale => { });
        parser.RegisterCommand((argument, index) => { });
        parser.Parse(["workflow.bonsai", "--property:Foo=Bar", "--editor-scale:2"]);
        CollectionAssert.AreEqual((int[])[1], parser.GetArgumentIndices(PropertyCommand).ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_ValueTokenLooksLikeCommand_NotConsumedAsValue()
    {
        var parser = CreateParser(out var values);
        parser.RegisterCommand("--start", () => { });
        parser.Parse(["--property", "--start"]);
        CollectionAssert.AreEqual((int[])[0], parser.GetArgumentIndices(PropertyCommand).ToArray());
        CollectionAssert.AreEqual((int[])[1], parser.GetArgumentIndices("--start").ToArray());
        CollectionAssert.AreEqual((string[])[""], values.ToArray());
    }

    [TestMethod]
    public void GetArgumentIndices_CommandNotSpecified_ReturnsEmpty()
    {
        var parser = CreateParser(out _);
        parser.Parse([]);
        Assert.AreEqual(0, parser.GetArgumentIndices(PropertyCommand).Count());
    }

    [TestMethod]
    public void GetArgumentIndices_UnregisteredOrNullCommand_ReturnsEmpty()
    {
        var parser = CreateParser(out _);
        parser.Parse(["--property:Foo=Bar"]);
        Assert.AreEqual(0, parser.GetArgumentIndices("--does-not-exist").Count());
        Assert.AreEqual(0, parser.GetArgumentIndices(null).Count());
    }

    [TestMethod]
    public void GetArgumentIndices_Reparse_ResetsState()
    {
        var parser = CreateParser(out _);
        parser.Parse(["--property:Foo=Bar"]);
        parser.Parse([]);
        Assert.AreEqual(0, parser.GetArgumentIndices(PropertyCommand).Count());
    }
}
