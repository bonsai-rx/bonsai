using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Design
{
    public class GraphNodeGrouping : Collection<GraphNode>, IGrouping<int, GraphNode>
    {
        public GraphNodeGrouping(int layer)
        {
            Key = layer;
        }

        public int Key { get; private set; }

        protected override void InsertItem(int index, GraphNode item)
        {
            item.LayerIndex = index;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, GraphNode item)
        {
            item.LayerIndex = index;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }
    }
}
