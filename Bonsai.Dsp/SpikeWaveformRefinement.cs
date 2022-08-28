namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies the spike waveform refinement method used in the
    /// <see cref="DetectSpikes"/> operator.
    /// </summary>
    public enum SpikeWaveformRefinement
    {
        /// <summary>
        /// The waveform is aligned to the first sample crossing the threshold.
        /// </summary>
        None,

        /// <summary>
        /// The waveform is aligned to either the positive or negative peak
        /// of the spike.
        /// </summary>
        AlignPeaks
    }
}
