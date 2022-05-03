using OpenTK;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a model matrix transform specifying
    /// relative position, rotation and scale to every transform in the sequence.
    /// </summary>
    [Description("Applies a model matrix transform specifying relative position, rotation and scale to every transform in the sequence.")]
    public class TransformRelative : MatrixTransform
    {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformRelative"/> class.
        /// </summary>
        public TransformRelative()
        {
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        /// <summary>
        /// Gets or sets the relative position of the model, in the local
        /// coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The relative position of the model, in the local coordinate frame.")]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets the quaternion representing the relative rotation of
        /// the model, in the local coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the relative rotation of the model, in the local coordinate frame.")]
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets the relative scale vector applied to the model, in the
        /// local coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The relative scale vector applied to the model, in the local coordinate frame.")]
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Initializes a transform matrix for applying a model matrix transform
        /// specifying relative position, rotation and scale.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateScale(ref scale, out result);
            Matrix4.CreateFromQuaternion(ref rotation, out Matrix4 temp);
            Matrix4.Mult(ref result, ref temp, out result);
            Matrix4.CreateTranslation(ref position, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
        }
    }
}
