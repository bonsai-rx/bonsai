using Bonsai.Expressions;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Design
{
    partial class TypeMappingEditorDialog : Form
    {
        static readonly object NullMapping = new object();
        CSharpCodeProvider provider;

        public TypeMappingEditorDialog(IEnumerable<TypeMapping> mappings)
        {
            InitializeComponent();
            provider = new CSharpCodeProvider();
            foreach (var mapping in mappings)
            {
                selectionListBox.Items.Add(mapping ?? NullMapping);
            }
        }

        public TypeConverter Converter { get; set; }

        public TypeMapping Mapping
        {
            get { return selectionListBox.SelectedItem as TypeMapping; }
            set
            {
                if (value != null)
                {
                    var selectedType = value.GetType().GetGenericArguments()[0];
                    for (int i = 0; i < selectionListBox.Items.Count; i++)
                    {
                        var mapping = selectionListBox.Items[i] as TypeMapping;
                        if (mapping != null && mapping.GetType().GetGenericArguments()[0] == selectedType)
                        {
                            selectionListBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else selectionListBox.SelectedIndex = -1;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            selectionListBox.ItemHeight = (int)(13 * factor.Height);
            base.ScaleControl(factor, specified);
        }

        private void selectionListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                var itemBounds = e.Bounds;
                var itemBrush = Brushes.Black;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    itemBrush = Brushes.White;
                }

                string itemText;
                var item = selectionListBox.Items[e.Index];
                if (item == NullMapping) itemText = string.Empty;
                else if (Converter != null) itemText = Converter.ConvertToString(item);
                else itemText = item.ToString();

                var itemExtent = (int)e.Graphics.MeasureString(itemText, e.Font).Width;
                selectionListBox.HorizontalExtent = Math.Max(selectionListBox.HorizontalExtent, itemExtent);
                e.Graphics.DrawString(itemText, e.Font, itemBrush, itemBounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        TreeNode CreateNode(TreeNodeCollection nodes, string name, Type type)
        {
            var typeRef = new CodeTypeReference(type);
            return nodes.Add(name, string.Format("{0} ({1})", name, provider.GetTypeOutput(typeRef)));
        }

        void InitializeMemberTree(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var root = treeView.Nodes.Add(provider.GetTypeOutput(new CodeTypeReference(type)));
            if (!type.IsArray)
            {
                type.VisitMember((member, memberType) => CreateNode(root.Nodes, member.Name, memberType));
                root.Expand();
            }
        }

        private void selectionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeView.Nodes.Clear();
            var mapping = selectionListBox.SelectedItem as TypeMapping;
            if (mapping != null)
            {
                var type = mapping.GetType().GetGenericArguments()[0];
                InitializeMemberTree(type);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            provider.Dispose();
            base.OnFormClosed(e);
        }
    }
}
