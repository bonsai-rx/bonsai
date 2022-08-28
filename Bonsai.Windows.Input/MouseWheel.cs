using System;
using System.ComponentModel;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that produces a sequence of values whenever the mouse wheel moves.
    /// </summary>
    [Description("Produces a sequence of values whenever the mouse wheel moves.")]
    public class MouseWheel : Source<int>
    {
        /// <summary>
        /// Generates an observable sequence of values whenever the mouse wheel moves.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="int"/> values representing the current
        /// scroll position whenever the mouse wheel moves.
        /// </returns>
        public override IObservable<int> Generate()
        {
            return InterceptMouse.Instance.MouseWheel;
        }
    }
}
