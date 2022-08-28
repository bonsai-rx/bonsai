using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that produces a value whenever a keyboard key is released.
    /// </summary>
    [DefaultProperty(nameof(Filter))]
    [Description("Produces a value whenever a keyboard key is released.")]
    public class KeyUp : Source<Keys>
    {
        /// <summary>
        /// Gets or sets the target keys to be observed.
        /// </summary>
        [Description("The target keys to be observed.")]
        public Keys Filter { get; set; }

        /// <summary>
        /// Returns an observable sequence that produces a value whenever a keyboard
        /// key is released.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Keys"/> values produced whenever a keyboard key
        /// is released.
        /// </returns>
        public override IObservable<Keys> Generate()
        {
            return InterceptKeys.Instance.KeyUp.Where(InterceptKeys.GetKeyFilter(Filter));
        }
    }
}
