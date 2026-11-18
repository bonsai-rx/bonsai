using System;

namespace Bonsai.Build
{
    static class Program
    {
        static int Main(string[] args)
        {
            return ScriptCompiler.Run(args, Environment.CurrentDirectory, Console.Out);
        }
    }
}
