using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.NuGet
{
    public class IncompatiblePackageException : InvalidOperationException
    {
        public IncompatiblePackageException(string message, IEnumerable<PackageDependencyConflict> dependencyConflicts)
            : base(message)
        {
            DependencyConflicts = dependencyConflicts.ToList().AsReadOnly();
        }

        public IReadOnlyList<PackageDependencyConflict> DependencyConflicts { get; }
    }
}
