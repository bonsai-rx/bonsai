using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Editor.GraphView
{
    class GraphNodeGrouping : Collection<GraphNode>, IGrouping<int, GraphNode>
    {
        public GraphNodeGrouping(int layer)
        {
            Key = layer;
            UpdateItems = true;
        }

        public int Key { get; private set; }

        public bool UpdateItems { get; set; }

        protected override void InsertItem(int index, GraphNode item)
        {
            base.InsertItem(index, item);
            if (UpdateItems)
            {
                item.LayerIndex = index;
                for (int i = index + 1; i < Count; i++)
                {
                    Items[i].LayerIndex = i;
                }
            }
        }

        protected override void SetItem(int index, GraphNode item)
        {
            base.SetItem(index, item);
            if (UpdateItems)
            {
                item.LayerIndex = index;
            }
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
