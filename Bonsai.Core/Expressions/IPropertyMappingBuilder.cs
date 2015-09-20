using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents expression builder instances that can dynamically remap input sources
    /// into object property assignments.
    /// </summary>
    public interface IPropertyMappingBuilder : IExpressionBuilder
    {
        /// <summary>
        /// Gets the collection of property mappings assigned to this expression builder.
        /// Property mapping subscriptions are processed before evaluating other output generation
        /// expressions.
        /// </summary>
        [Obsolete]
        PropertyMappingCollection PropertyMappings { get; }
    }
}
