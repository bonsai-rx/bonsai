using System.Globalization;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents the result of a camera calibration algorithm, including
    /// the camera intrinsic parameters and an estimate of the re-projection
    /// error.
    /// </summary>
    public struct CameraCalibration
    {
        /// <summary>
        /// The parameters that describe the camera intrinsic properties such
        /// as the focal length or lens distortion.
        /// </summary>
        public Intrinsics Intrinsics;

        /// <summary>
        /// The final re-projection error of the camera calibration.
        /// </summary>
        public double ReprojectionError;

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="CameraCalibration"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="CameraCalibration"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{Intrinsics: {0}, ReprojectionError: {1}}}",
                Intrinsics,
                ReprojectionError);
        }
    }
}
