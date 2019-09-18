using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Bonsai.Resources.Design
{
    partial class CollectionEditorControl : UserControl
    {
        int addTop;
        int removeTop;
        int initialHeight;
        int initialListHeight;
        const int ButtonMargin = 2;
        static readonly object AfterExpandEvent = new object();
        static readonly object BeforeExpandEvent = new object();
        static readonly object SelectedItemChangedEvent = new object();
        Lazy<XmlSerializer> itemCollectionSerializer;

        public CollectionEditorControl()
        {
            InitializeComponent();
        }

        public Type CollectionItemType { get; set; }

        public Type[] NewItemTypes { get; set; }

        public override bool AllowDrop
        {
            get { return selectionListBox.AllowDrop; }
            set { selectionListBox.AllowDrop = value; }
        }

        public IEnumerable Items
        {
            get { return selectionListBox.Items; }
            set
            {
                selectionListBox.Items.Clear();
                foreach (var item in value)
                {
                    selectionListBox.Items.Add(item);
                }
            }
        }

        public object SelectedItem
        {
            get { return selectionListBox.SelectedItem; }
            set { selectionListBox.SelectedItem = value; }
        }

        public IEnumerable SelectedItems
        {
            get { return selectionListBox.SelectedItems; }
        }

        public event EventHandler SelectedItemChanged
        {
            add { Events.AddHandler(SelectedItemChangedEvent, value); }
            remove { Events.RemoveHandler(SelectedItemChangedEvent, value); }
        }

        void SetExpanded(bool expanded)
        {
            selectionListBox.Visible = expanded;
            addButton.Visible = expanded;
            removeButton.Visible = expanded;
            upButton.Visible = expanded;
            downButton.Visible = expanded;
            OnSelectedItemChanged(EventArgs.Empty);
        }

        private void OnSelectedItemChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[SelectedItemChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            addTop = addButton.Top;
            removeTop = removeButton.Top;
            initialHeight = Height;
            initialListHeight = selectionListBox.Height;
            itemCollectionSerializer = new Lazy<XmlSerializer>(() =>
                new XmlSerializer(typeof(List<>).MakeGenericType(CollectionItemType)));
            var itemTypes = NewItemTypes ?? new[] { CollectionItemType };
            if (itemTypes.Length > 1)
            {
                var menuStrip = new ContextMenuStrip();
                foreach (var type in itemTypes)
                {
                    var itemType = type;
                    var displayNameAttributes = (DisplayNameAttribute[])type.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    var displayName = displayNameAttributes.Length > 0 ? displayNameAttributes[0].DisplayName : type.Name;
                    menuStrip.Items.Add(displayName, null, delegate
                    {
                        var item = CreateInstance(itemType);
                        AddItem(item);
                    });
                }
                addButton.ContextMenuStrip = menuStrip;
            }
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            selectionListBox.ItemHeight = (int)(13 * factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void OnResize(EventArgs e)
        {
            if (initialHeight > 0)
            {
                var expansion = Height - initialHeight;
                selectionListBox.Height = initialListHeight + expansion;
                addButton.Top = addTop + expansion;
                removeButton.Top = removeTop + expansion;
            }
            base.OnResize(e);
        }

        protected virtual object CreateInstance(Type itemType)
        {
            return Activator.CreateInstance(itemType);
        }

        public void AddItem(object item)
        {
            if (item != null)
            {
                var index = selectionListBox.SelectedIndex + 1;
                if (index == 0) index = selectionListBox.Items.Count;
                selectionListBox.Items.Insert(index, item);
                selectionListBox.SelectedItem = null;
                selectionListBox.SelectedIndex = index;
            }
        }

        private void RemoveSelectedItems()
        {
            if (selectionListBox.SelectedIndices.Count > 0)
            {
                var selectedIndex = selectionListBox.SelectedIndex;
                for (int i = selectionListBox.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    var index = selectionListBox.SelectedIndices[i];
                    selectionListBox.Items.RemoveAt(index);
                }

                selectionListBox.SelectedIndex = Math.Min(selectedIndex, selectionListBox.Items.Count - 1);
                selectionListBox.HorizontalExtent = 0;
            }
        }

        private void SwapItemIndex(int index, int newIndex)
        {
            if (newIndex >= 0 && newIndex < selectionListBox.Items.Count)
            {
                var temp = selectionListBox.Items[newIndex];
                selectionListBox.Items[newIndex] = selectionListBox.Items[index];
                selectionListBox.Items[index] = temp;
            }
        }

        private void MoveSelectedIndicesUp()
        {
            if (selectionListBox.SelectedIndices.Count > 0)
            {
                if (selectionListBox.SelectedIndices[0] <= 0) return;
                var indices = new int[selectionListBox.SelectedIndices.Count];
                selectionListBox.SelectedIndices.CopyTo(indices, 0);
                selectionListBox.SelectedItem = null;

                for (int i = 0; i < indices.Length; i++)
                {
                    var newIndex = indices[i] - 1;
                    SwapItemIndex(indices[i], newIndex);
                    selectionListBox.SelectedIndices.Add(newIndex);
                }
            }
        }

        private void MoveSelectedIndicesDown()
        {
            var selectedIndices = selectionListBox.SelectedIndices;
            if (selectedIndices.Count > 0)
            {
                if (selectedIndices[selectedIndices.Count - 1] >= selectionListBox.Items.Count - 1) return;
                var indices = new int[selectedIndices.Count];
                selectionListBox.SelectedIndices.CopyTo(indices, 0);
                selectionListBox.SelectedItem = null;

                for (int i = indices.Length - 1; i >= 0; i--)
                {
                    var newIndex = indices[i] + 1;
                    SwapItemIndex(indices[i], newIndex);
                    selectionListBox.SelectedIndices.Add(newIndex);
                }
            }
        }

        private void SelectAll()
        {
            for (int i = 0; i < selectionListBox.Items.Count; i++)
            {
                selectionListBox.SetSelected(i, true);
            }
        }

        public void Cut()
        {
            Copy();
            RemoveSelectedItems();
        }

        public void Copy()
        {
            if (selectionListBox.SelectedItems.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings { Indent = true }))
                {
                    var serializer = itemCollectionSerializer.Value;
                    var items = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(CollectionItemType));
                    foreach (var item in selectionListBox.SelectedItems)
                    {
                        items.Add(item);
                    }

                    serializer.Serialize(writer, items);
                }

                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        protected virtual bool DeserializeItems(XmlReader reader)
        {
            var serializer = itemCollectionSerializer.Value;
            if (serializer.CanDeserialize(reader))
            {
                var items = (IList)serializer.Deserialize(reader);
                foreach (var item in items)
                {
                    AddItem(item);
                }

                return true;
            }

            return false;
        }

        public void Paste()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var stringReader = new StringReader(text);
                using (var reader = XmlReader.Create(stringReader))
                {
                    try { DeserializeItems(reader); }
                    catch (XmlException) { }
                    catch (InvalidOperationException) { }
                }
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var itemTypes = NewItemTypes;
            var itemType = itemTypes != null && itemTypes.Length > 0 ? itemTypes[0] : CollectionItemType;
            var item = CreateInstance(itemType);
            AddItem(item);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedItems();
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            MoveSelectedIndicesUp();
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            MoveSelectedIndicesDown();
        }

        private void selectionListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                Copy();
            }

            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                Paste();
            }

            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                e.SuppressKeyPress = true;
                SelectAll();
            }

            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItems();
            }
        }

        private void selectionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedItemChanged(e);
        }

        private void selectionListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                var buttonBounds = e.Bounds;
                var maxIndex = selectionListBox.Items.Count - 1;
                buttonBounds.Width = (int)e.Graphics.MeasureString(maxIndex.ToString(), e.Font).Width;
                buttonBounds.Width += 2 * ButtonMargin + 1;
                ControlPaint.DrawButton(e.Graphics, buttonBounds, ButtonState.Inactive);

                var indexWidth = (int)e.Graphics.MeasureString(e.Index.ToString(), e.Font).Width;
                buttonBounds.X = (buttonBounds.Width - indexWidth) / 2;
                e.Graphics.DrawString(e.Index.ToString(), e.Font, Brushes.Black, buttonBounds.Location);

                var itemBounds = e.Bounds;
                itemBounds.X += buttonBounds.Width;

                var itemBrush = Brushes.Black;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    itemBrush = Brushes.White;
                }

                var itemText = selectionListBox.Items[e.Index].ToString();
                var itemExtent = (int)e.Graphics.MeasureString(itemText, e.Font).Width + buttonBounds.Width;
                selectionListBox.HorizontalExtent = Math.Max(selectionListBox.HorizontalExtent, itemExtent);
                e.Graphics.DrawString(itemText, e.Font, itemBrush, itemBounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void selectionListBox_DragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        private void selectionListBox_DragEnter(object sender, DragEventArgs e)
        {
            OnDragEnter(e);
        }
    }
}
