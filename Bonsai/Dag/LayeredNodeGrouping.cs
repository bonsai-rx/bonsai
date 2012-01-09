using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Dag
{
    public class LayeredNodeGrouping<TValue, TLabel> : Collection<LayeredNode<TValue, TLabel>>, IGrouping<int, LayeredNode<TValue, TLabel>>
    {
        public LayeredNodeGrouping(int layer)
        {
            Key = layer;
        }

        public int Key { get; private set; }

        protected override void InsertItem(int index, LayeredNode<TValue, TLabel> item)
        {
            item.LayerIndex = index;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, LayeredNode<TValue, TLabel> item)
        {
            Items[index].LayerIndex = -1;
            item.LayerIndex = index;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Items[index].LayerIndex = -1;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.LayerIndex = -1;
            }
            base.ClearItems();
        }
    }
}
