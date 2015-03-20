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

        void AddMember(Type type);

        void AddMember(string name, Type type);
    }
}
