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

namespace Bonsai.Shaders.Configuration.Design
{
    partial class CollectionEditorControl : UserControl
    {
        int addTop;
        int removeTop;
        int initialHeight;
        int initialListHeight;
        const int ButtonMargin = 2;
        const float DefaultDpi = 96f;
        const float DefaultFontSize = 8.25f;
        static readonly object AfterExpandEvent = new object();
        static readonly object BeforeExpandEvent = new object();
        static readonly object SelectedItemChangedEvent = new object();

        public CollectionEditorControl()
        {
            InitializeComponent();
        }

        public Type CollectionItemType { get; set; }

        public Type[] NewItemTypes { get; set; }

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
            var itemTypes = NewItemTypes ?? new[] { CollectionItemType };
            if (itemTypes.Length > 1)
            {
                var menuStrip = new ContextMenuStrip();
                foreach (var type in itemTypes)
                {
                    var itemType = type;
                    menuStrip.Items.Add(type.Name, null, delegate
                    {
                        var item = CreateInstance(itemType);
                        AddItem(item);
                    });
                }
                addButton.ContextMenuStrip = menuStrip;
            }
            UpdateDrawScale();
            base.OnLoad(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            UpdateDrawScale();
            base.OnFontChanged(e);
        }

        void UpdateDrawScale()
        {
            using (var graphics = CreateGraphics())
            {
                var drawScale = Font.Size / DefaultFontSize * graphics.DpiY / DefaultDpi;
                selectionListBox.ItemHeight = (int)(13 * drawScale);
            }
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

        private void AddItem(object item)
        {
            if (item != null)
            {
                var index = selectionListBox.SelectedIndex + 1;
                if (index == 0) index = selectionListBox.Items.Count;
                selectionListBox.Items.Insert(index, item);
                selectionListBox.SelectedIndex = index;
            }
        }

        private void RemoveSelectedItem()
        {
            if (selectionListBox.SelectedItem != null)
            {
                var selectedIndex = selectionListBox.SelectedIndex;
                selectionListBox.Items.RemoveAt(selectedIndex);
                selectionListBox.SelectedIndex = Math.Min(selectedIndex, selectionListBox.Items.Count - 1);
                selectionListBox.HorizontalExtent = 0;
            }
        }

        private void SwapSelectedIndex(int newIndex)
        {
            var selectedIndex = selectionListBox.SelectedIndex;
            if (newIndex >= 0 && newIndex < selectionListBox.Items.Count)
            {
                var temp = selectionListBox.Items[newIndex];
                selectionListBox.Items[newIndex] = selectionListBox.SelectedItem;
                selectionListBox.Items[selectedIndex] = temp;
                selectionListBox.SelectedIndex = newIndex;
            }
        }

        private void CopySelectedItem()
        {
            if (selectionListBox.SelectedItem != null)
            {
                var stringBuilder = new StringBuilder();
                using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings { Indent = true }))
                {
                    var serializer = new XmlSerializer(CollectionItemType);
                    serializer.Serialize(writer, selectionListBox.SelectedItem);
                }

                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        private void PasteCollectionItem()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var stringReader = new StringReader(text);
                using (var reader = XmlReader.Create(stringReader))
                {
                    try
                    {
                        var serializer = new XmlSerializer(CollectionItemType);
                        if (serializer.CanDeserialize(reader))
                        {
                            var item = serializer.Deserialize(reader);
                            AddItem(item);
                        }
                    }
                    catch (XmlException) { }
                    catch (InvalidOperationException) { }
                }
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var item = CreateInstance(CollectionItemType);
            AddItem(item);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            SwapSelectedIndex(selectionListBox.SelectedIndex - 1);
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            SwapSelectedIndex(selectionListBox.SelectedIndex + 1);
        }

        private void selectionListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                CopySelectedItem();
            }

            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                PasteCollectionItem();
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
    }
}
