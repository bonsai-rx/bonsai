namespace Bonsai.Vision.Design
{
    public class IplImageInputLabeledRoiEditor : IplImageRoiEditor
    {
        public IplImageInputLabeledRoiEditor()
            : base(DataSource.Input)
        {
            LabelRegions = true;
        }
    }
}
