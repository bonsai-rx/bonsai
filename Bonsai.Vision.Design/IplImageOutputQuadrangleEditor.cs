namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a quadrangular region on top
    /// of the output image sequence.
    /// </summary>
    public class IplImageOutputQuadrangleEditor : IplImageQuadrangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputQuadrangleEditor"/> class.
        /// </summary>
        public IplImageOutputQuadrangleEditor()
            : base(DataSource.Output)
        {
        }
    }
}
