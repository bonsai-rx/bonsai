namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a rectangular region on top
    /// of the output image sequence.
    /// </summary>
    public class IplImageOutputRectangleEditor : IplImageRectangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputRectangleEditor"/> class.
        /// </summary>
        public IplImageOutputRectangleEditor()
            : base(DataSource.Output)
        {
        }
    }
}
