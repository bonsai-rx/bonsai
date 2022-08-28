using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface with a scaled description panel that can edit
    /// most types of collections at design time.
    /// </summary>
    public class DescriptiveCollectionEditor : CollectionEditor
    {
        const float DefaultDpi = 96f;

        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptiveCollectionEditor"/>
        /// class using the specified type.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public DescriptiveCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            ActivateDescription(form);
            return form;
        }

        static void ActivateDescription(Control control)
        {
            var propertyGrid = control as System.Windows.Forms.PropertyGrid;
            if (propertyGrid != null)
            {
                propertyGrid.HelpVisible = true;
                using (var graphics = propertyGrid.CreateGraphics())
                {
                    var drawScale = graphics.DpiY / DefaultDpi *
                                    propertyGrid.Font.SizeInPoints / Control.DefaultFont.SizeInPoints;
                    PropertyGrid.ScaleDescriptionPanel(propertyGrid, new SizeF(drawScale, drawScale));
                }
            }

            foreach (Control child in control.Controls)
            {
                ActivateDescription(child);
            }
        }
    }
}
