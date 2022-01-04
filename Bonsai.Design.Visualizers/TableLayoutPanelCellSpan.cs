using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Represents the vertical and horizontal span of a table layout cell.
    /// </summary>
    public class TableLayoutPanelCellSpan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableLayoutPanelCellSpan"/> class.
        /// </summary>
        public TableLayoutPanelCellSpan()
        {
            ColumnSpan = 1;
            RowSpan = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableLayoutPanelCellSpan"/> class
        /// using the specified column and row span.
        /// </summary>
        /// <param name="columnSpan">The number of columns spanned by the table layout cell.</param>
        /// <param name="rowSpan">The number of rows spanned by the table layout cell.</param>
        public TableLayoutPanelCellSpan(int columnSpan, int rowSpan)
        {
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }

        /// <summary>
        /// Gets the number of columns spanned by this table layout cell.
        /// </summary>
        [XmlAttribute]
        public int ColumnSpan { get; set; }

        /// <summary>
        /// Gets the number of rows spanned by this table layout cell.
        /// </summary>
        [XmlAttribute]
        public int RowSpan { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ColumnSpan = {ColumnSpan}, RowSpan = {RowSpan}";
        }
    }
}
