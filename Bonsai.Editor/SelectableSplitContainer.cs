using System.Windows.Forms;

namespace Bonsai.Editor
{
    class SelectableSplitContainer : SplitContainer
    {
        public bool Selectable
        {
            get { return GetStyle(ControlStyles.Selectable); }
            set { SetStyle(ControlStyles.Selectable, value); }
        }
    }
}
