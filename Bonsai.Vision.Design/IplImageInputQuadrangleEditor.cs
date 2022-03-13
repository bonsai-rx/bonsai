namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a quadrangular region on top
    /// of the input image sequence.
    /// </summary>
    public class IplImageInputQuadrangleEditor : IplImageQuadrangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputQuadrangleEditor"/> class.
        /// </summary>
        public IplImageInputQuadrangleEditor()
            : base(DataSource.Input)
        {
        }
    }
}
