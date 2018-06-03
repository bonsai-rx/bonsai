using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    interface IMemberSelectorEditorDialog : IDisposable
    {
        string Selector { get; set; }
    }
}
