using System;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a set of sparse correspondences between features detected in a
    /// reference image, and matching features detected in the current image.
    /// </summary>
    public class KeyPointOpticalFlow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPointOpticalFlow"/> class
        /// using the set of sparse feature correspondences detected in the reference
        /// and current images.
        /// </summary>
        /// <param name="previousKeyPoints">
        /// The set of features detected in the reference image.
        /// </param>
        /// <param name="currentKeyPoints">
        /// The set of matching features detected in the current image.
        /// </param>
        public KeyPointOpticalFlow(KeyPointCollection previousKeyPoints, KeyPointCollection currentKeyPoints)
        {
            PreviousKeyPoints = previousKeyPoints ?? throw new ArgumentNullException(nameof(previousKeyPoints));
            CurrentKeyPoints = currentKeyPoints ?? throw new ArgumentNullException(nameof(currentKeyPoints));
        }

        /// <summary>
        /// Gets the set of features detected in the reference image.
        /// </summary>
        public KeyPointCollection PreviousKeyPoints { get; private set; }

        /// <summary>
        /// Gets the set of matching features detected in the current image.
        /// </summary>
        public KeyPointCollection CurrentKeyPoints { get; private set; }
    }
}
