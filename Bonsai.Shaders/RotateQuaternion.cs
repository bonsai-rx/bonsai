using OpenTK;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a rotation specified by a quaternion
    /// to every transform in the sequence.
    /// </summary>
    [Description("Applies a rotation specified by a quaternion to every transform in the sequence.")]
    public class RotateQuaternion : MatrixTransform
    {
        Quaternion rotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotateQuaternion"/> class.
        /// </summary>
        public RotateQuaternion()
        {
            rotation = Quaternion.Identity;
        }

        /// <summary>
        /// Gets or sets the quaternion representing the rotation transform.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the rotation transformation.")]
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Initializes a transform matrix for applying a rotation specified by
        /// a quaternion.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateFromQuaternion(ref rotation, out result);
        }
    }
}
