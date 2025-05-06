using System;
using System.Text.RegularExpressions;

namespace Bonsai.Configuration.Tests;

internal static class TestHelpers
{
    /// <summary>Normalizes newlines to <see cref="Environment.NewLine"/>, intended for use with raw strings when newlines matter</summary>
    internal static string NormalizeNewlines(this string value)
        => Regex.Replace(value, "\r?\n", Environment.NewLine);
}
