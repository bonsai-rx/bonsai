namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies the method used to rescale values in the <see cref="Rescale"/> operator.
    /// </summary>
    public enum RescaleMethod
    {
        /// <summary>
        /// Values outside the specified range are extrapolated linearly.
        /// </summary>
        Linear,

        /// <summary>
        /// Values outside the specified range are clamped to the lower or upper
        /// bounds of the output range.
        /// </summary>
        Clamp
    }
}
