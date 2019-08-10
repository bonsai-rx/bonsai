using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Specifies the file name filter which determines the choices that appear in the file type
    /// selection box of a file dialog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FileNameFilterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileNameFilterAttribute"/> class with
        /// the specified file name filter.
        /// </summary>
        /// <param name="filter">
        /// The file name filter string which determines the choices that appear in the file type
        /// selection box of a file dialog.
        /// </param>
        public FileNameFilterAttribute(string filter)
        {
            Filter = filter;
        }

        /// <summary>
        /// Gets the file name filter string used to determine the choices in the file type
        /// selection box of a file dialog.
        /// </summary>
        public string Filter { get; private set; }
    }
}
