using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Represents workflow elements that have a name.
    /// </summary>
    public interface INamedElement
    {
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        string Name { get; }
    }
}
