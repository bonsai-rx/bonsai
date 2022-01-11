namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies the downsampling method used to decimate digital signals.
    /// </summary>
    public enum DownsamplingMethod
    {
        /// <summary>
        /// No preprocessing will be used before decimating the signal. The
        /// downsampled signal will keep only every Mth sample, where M
        /// is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        None,

        /// <summary>
        /// A low-pass filter will be applied to the signal before downsampling.
        /// </summary>
        LowPass,

        /// <summary>
        /// The downsampled signal will keep a random sample out of every
        /// M samples, where M is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        Dithering,

        /// <summary>
        /// The downsampled signal will keep the sum of every M samples, where
        /// M is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        Sum,

        /// <summary>
        /// The downsampled signal will keep the average of every M samples, where
        /// M is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        Avg,

        /// <summary>
        /// The downsampled signal will keep the largest of every M samples, where
        /// M is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        Max,

        /// <summary>
        /// The downsampled signal will keep the smallest of every M samples, where
        /// M is the integral factor in <see cref="Decimate.Factor"/>.
        /// </summary>
        Min
    }
}
