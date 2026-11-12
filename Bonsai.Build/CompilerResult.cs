using System;
using System.Collections.Generic;

namespace Bonsai.Build
{
    /// <summary>
    /// Represents the result of compiling a set of source files into an assembly.
    /// </summary>
    public sealed class CompilerResult
    {
        internal CompilerResult(bool success, IReadOnlyList<string> diagnostics)
        {
            Success = success;
            Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        /// <summary>
        /// Gets a value indicating whether the assembly was compiled and written successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the errors and warnings reported by the compiler, formatted with the path and
        /// position of the source they refer to.
        /// </summary>
        public IReadOnlyList<string> Diagnostics { get; }
    }
}
