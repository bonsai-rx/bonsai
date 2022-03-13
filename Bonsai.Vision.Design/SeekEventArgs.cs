using System;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides data for the <see cref="VideoPlayer.Seek"/> event.
    /// </summary>
    public class SeekEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeekEventArgs"/> class
        /// using the specified frame number.
        /// </summary>
        /// <param name="frameNumber">
        /// The zero-based index of the frame the player should move to.
        /// </param>
        public SeekEventArgs(int frameNumber)
        {
            FrameNumber = frameNumber;
        }

        /// <summary>
        /// Gets the zero-based index of the frame the player should move to.
        /// </summary>
        public int FrameNumber { get; private set; }
    }
}
