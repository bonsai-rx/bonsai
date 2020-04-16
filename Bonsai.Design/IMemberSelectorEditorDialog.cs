using System;

namespace Bonsai.Design
{
    interface IMemberSelectorEditorDialog : IDisposable
    {
        string Selector { get; set; }
    }
}
