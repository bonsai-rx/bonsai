using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    public class TableLayoutPanelCellSpan
    {
        public TableLayoutPanelCellSpan()
        {
            ColumnSpan = 1;
            RowSpan = 1;
        }

        public TableLayoutPanelCellSpan(int columnSpan, int rowSpan)
        {
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }

        [XmlAttribute]
        public int ColumnSpan { get; set; }

        [XmlAttribute]
        public int RowSpan { get; set; }

        public override string ToString()
        {
            return $"ColumnSpan = {ColumnSpan}, RowSpan = {RowSpan}";
        }
    }
}
