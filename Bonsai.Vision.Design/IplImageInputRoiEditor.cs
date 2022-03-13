namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a collection of polygonal
    /// regions on top of the input image sequence.
    /// </summary>
    public class IplImageInputRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputRoiEditor"/> class.
        /// </summary>
        public IplImageInputRoiEditor()
            : base(DataSource.Input)
        {
        }
    }
}
