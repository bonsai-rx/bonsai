using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that produces a value whenever a keyboard key is pressed.
    /// </summary>
    [DefaultProperty(nameof(Filter))]
    [Description("Produces a value whenever a keyboard key is pressed.")]
    public class KeyDown : Source<Keys>
    {
        /// <summary>
        /// Gets or sets the target keys to be observed.
        /// </summary>
        [Description("The target keys to be observed.")]
        public Keys Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore character repetitions
        /// when a key is held down.
        /// </summary>
        [Description("Indicates whether to ignore character repetitions when a key is held down.")]
        public bool SuppressRepetitions { get; set; }

        /// <summary>
        /// Returns an observable sequence that produces a value whenever a keyboard
        /// key is pressed.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Keys"/> values produced whenever a keyboard key
        /// is pressed.
        /// </returns>
        public override IObservable<Keys> Generate()
        {
            var predicate = InterceptKeys.GetKeyFilter(Filter);
            var source = InterceptKeys.Instance.KeyDown.Where(predicate);
            if (SuppressRepetitions)
            {
                source = source
                    .Window(() => InterceptKeys.Instance.KeyUp.Where(predicate))
                    .SelectMany(sequence => sequence.Take(1));
            }
            return source;
        }
    }
}
