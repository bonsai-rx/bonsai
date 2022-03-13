namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a rectangular region on top
    /// of the input image sequence.
    /// </summary>
    public class IplImageInputRectangleEditor : IplImageRectangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputRectangleEditor"/> class.
        /// </summary>
        public IplImageInputRectangleEditor()
            : base(DataSource.Input)
        {
        }
    }
}
