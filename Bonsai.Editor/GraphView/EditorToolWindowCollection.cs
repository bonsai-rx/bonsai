using System.Collections.ObjectModel;
using Bonsai.Editor.Docking;

namespace Bonsai.Editor.GraphView
{
    internal class EditorToolWindowCollection : KeyedCollection<string, EditorToolWindow>
    {
        protected override string GetKeyForItem(EditorToolWindow item)
        {
            return item.GetType().Name;
        }
    }
}
