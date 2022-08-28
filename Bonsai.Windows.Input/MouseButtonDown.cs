using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that produces a value whenever a mouse button is pressed.
    /// </summary>
    [DefaultProperty(nameof(Filter))]
    [Description("Produces a value whenever a mouse button is pressed.")]
    public class MouseButtonDown : Source<MouseButtons>
    {
        /// <summary>
        /// Gets or sets the target buttons to be observed.
        /// </summary>
        [Description("The target buttons to be observed.")]
        public MouseButtons Filter { get; set; }

        /// <summary>
        /// Returns an observable sequence that produces a value whenever a mouse
        /// button is pressed.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="MouseButtons"/> values produced whenever a mouse
        /// button is pressed.
        /// </returns>
        public override IObservable<MouseButtons> Generate()
        {
            return InterceptMouse.Instance.MouseButtonDown
                .Where(button => Filter == MouseButtons.None || button == Filter);
        }
    }
}
