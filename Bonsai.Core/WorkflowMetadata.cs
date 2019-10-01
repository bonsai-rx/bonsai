using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai
{
    /// <summary>
    /// Represents metadata retrieved from a serializable XML workflow.
    /// </summary>
    public class WorkflowMetadata
    {
        internal bool Legacy { get; set; }

        internal HashSet<Type> Types { get; set; }

        /// <summary>
        /// Gets the description of the workflow.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the raw XML markup of the workflow.
        /// </summary>
        public string WorkflowMarkup { get; internal set; }

        /// <summary>
        /// Gets the extension types required to deserialize the XML markup.
        /// </summary>
        /// <returns>
        /// An array that contains all the types that are required to deserialize the XML markup.
        /// </returns>
        public Type[] GetExtensionTypes()
        {
            System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            return (Types ?? Enumerable.Empty<Type>()).ToArray();
        }
    }
}
