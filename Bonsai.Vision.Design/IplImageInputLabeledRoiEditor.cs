using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
