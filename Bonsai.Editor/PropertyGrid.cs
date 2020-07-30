using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class PropertyGrid : Design.PropertyGrid
    {
        static readonly object EventRefreshed = new object();

        public event EventHandler Refreshed
        {
            add { Events.AddHandler(EventRefreshed, value); }
            remove { Events.RemoveHandler(EventRefreshed, value); }
        }

        public override void Refresh()
        {
            base.Refresh();
            (Events[EventRefreshed] as EventHandler)?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPropertyValueChanged(PropertyValueChangedEventArgs e)
        {
            base.OnPropertyValueChanged(e);
            if (e.ChangedItem.GridItemType == GridItemType.Property)
            {
                var refreshProperties = (RefreshPropertiesAttribute)e.ChangedItem.PropertyDescriptor.Attributes[typeof(RefreshPropertiesAttribute)];
                if (refreshProperties != null && refreshProperties.RefreshProperties == RefreshProperties.All)
                {
                    Refresh();
                }
            }
        }
    }
}
