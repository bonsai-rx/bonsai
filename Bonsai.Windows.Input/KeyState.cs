using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that determines whether a key is up or down at the time of notification.
    /// </summary>
    [DefaultProperty(nameof(Filter))]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Determines whether a key is up or down at the time of notification.")]
    public class KeyState : Combinator<bool>
    {
        /// <summary>
        /// Gets or sets the target key to be observed.
        /// </summary>
        [Description("The target key to be observed.")]
        public Keys Filter { get; set; }

        /// <summary>
        /// Generates a sequence of values indicating whether the target key is up or down
        /// each time the <paramref name="source"/> sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to check whether the key
        /// is up or down.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the key is up or down
        /// at the time the <paramref name="source"/> sequence emits a notification.
        /// </returns>
        public override IObservable<bool> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => InterceptKeys.GetKeyState(Filter));
        }
    }
}
