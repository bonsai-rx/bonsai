namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies the type of digital pass filter to apply on a signal.
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// A low-pass filter rejects frequencies above the cutoff frequency.
        /// </summary>
        LowPass,

        /// <summary>
        /// A high-pass filter rejects frequencies below the cutoff frequency.
        /// </summary>
        HighPass,

        /// <summary>
        /// A band-pass filter rejects frequencies outside the specified frequency
        /// range, i.e. frequencies below the first cutoff frequency and above the
        /// second cutoff frequency are rejected.
        /// </summary>
        BandPass,

        /// <summary>
        /// A band-stop filter rejects frequencies within the specified frequency
        /// range, i.e. frequencies between the first cutoff frequency and the
        /// second cutoff frequency are rejected.
        /// </summary>
        BandStop
    }
}
