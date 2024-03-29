﻿namespace Bonsai
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

        /// <summary>
        /// The assembly qualified name of a UI editor that can edit multiline string
        /// values using a text box. This field is read-only.
        /// </summary>
        public const string MultilineStringEditor = "System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        /// <summary>
        /// The assembly qualified name of a UI editor that can edit folder name paths
        /// using a folder browser dialog. This field is read-only.
        /// </summary>
        public const string FolderNameEditor = "Bonsai.Design.FolderNameEditor, Bonsai.Design";

        /// <summary>
        /// The assembly qualified name of a UI editor that can edit file name paths
        /// using an open file dialog. This field is read-only.
        /// </summary>
        public const string OpenFileNameEditor = "Bonsai.Design.OpenFileNameEditor, Bonsai.Design";

        /// <summary>
        /// The assembly qualified name of a UI editor that can edit file name paths
        /// using a save file dialog. This field is read-only.
        /// </summary>
        public const string SaveFileNameEditor = "Bonsai.Design.SaveFileNameEditor, Bonsai.Design";

        /// <summary>
        /// The assembly qualified name of the UI editor base class. This field is read-only.
        /// </summary>
        public const string UITypeEditor = "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    }
}
