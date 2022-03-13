namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing a collection of labeled
    /// polygonal regions on top of the active image source.
    /// </summary>
    public class IplImageInputLabeledRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputLabeledRoiEditor"/> class.
        /// </summary>
        public IplImageInputLabeledRoiEditor()
            : base(DataSource.Input)
        {
            LabelRegions = true;
        }
    }
}
