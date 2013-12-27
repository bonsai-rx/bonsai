using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Provides assembly qualified names for well-known designer types.
    /// </summary>
    public static class DesignTypes
    {
        /// <summary>
        /// The assembly qualified name of a UI editor that can edit numeric values
        /// using a visual slider bar. This field is read-only.
        /// </summary>
        public const string SliderEditor = "Bonsai.Design.SliderEditor, Bonsai.Design";

        /// <summary>
        /// The assembly qualified name of a UI editor that can edit numeric values
        /// using a numeric spin box. This field is read-only.
        /// </summary>
        public const string NumericUpDownEditor = "Bonsai.Design.NumericUpDownEditor, Bonsai.Design";
    }
}
