using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.IO
{
    /// <summary>
    /// Specifies the known path suffixes that can be appended to a path.
    /// </summary>
    public enum PathSuffix
    {
        /// <summary>
        /// Specifies that no suffix should be appended to the path.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that the suffix should be the number of files in the same
        /// directory that start with the same file name.
        /// </summary>
        FileCount,

        /// <summary>
        /// Specifies that the suffix should be the current timestamp as generated
        /// by a high resolution system timer, if available.
        /// </summary>
        Timestamp
    }
}
