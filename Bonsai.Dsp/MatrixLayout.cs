namespace Bonsai.Dsp
{
    /// <summary>
    /// Specifies how to store the elements of a multi-channel array or matrix
    /// in sequential memory.
    /// </summary>
    public enum MatrixLayout
    {
        /// <summary>
        /// The elements in each row of the matrix will be contiguous in memory.
        /// </summary>
        RowMajor,

        /// <summary>
        /// The elements in each column of the matrix will be contiguous in memory.
        /// </summary>
        ColumnMajor
    }
}
