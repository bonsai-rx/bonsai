namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies the periodic function used to generate a signal waveform.
    /// </summary>
    public enum FunctionWaveform
    {
        /// <summary>
        /// A sine wave describing a smooth periodic oscillation.
        /// </summary>
        Sine,

        /// <summary>
        /// A periodic waveform in which the amplitude alternates between
        /// fixed minimum and maximum values.
        /// </summary>
        Square,

        /// <summary>
        /// A periodic, piecewise linear waveform in which the amplitude
        /// moves between fixed minimum and maximum values following a
        /// triangular shape.
        /// </summary>
        Triangular,

        /// <summary>
        /// A periodic, non-symmetric and piecewise linear waveform in which
        /// the amplitude moves linearly between the fixed minimum and maximum
        /// values, and then drops sharply from the maximum back to the minimum
        /// value.
        /// </summary>
        Sawtooth
    }
}
