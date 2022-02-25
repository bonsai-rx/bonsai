using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a value that will be assigned to the specified camera or video
    /// file property upon initialization.
    /// </summary>
    public class CapturePropertyAssignment
    {
        /// <summary>
        /// Gets or sets the property that the value will be assigned to.
        /// </summary>
        public CaptureProperty Property { get; set; }

        /// <summary>
        /// Gets or sets the value to be assigned to the property.
        /// </summary>
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public double Value { get; set; }


        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="CapturePropertyAssignment"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the name of the property and the value
        /// to be assigned by this <see cref="CapturePropertyAssignment"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} = {1}", Property, Value);
        }
    }
}
