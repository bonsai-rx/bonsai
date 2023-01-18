using Bonsai;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{ 
    /// <summary>
    /// Represents an operator that returns the value of an OS environment variable.
    /// </summary>
    /// 
    [Description("Returns the value of an environment variable.")]
    public class GetEnvironmentVariable : Source<string>
    {
        [Description("The name of the environment variable to query the value of.")]
        public string Variable { get; set; }

        /// <summary>
        /// Generates an observable sequence containing a string with the value
        /// of the queried environment variable.
        /// </summary>
        /// <returns>
        /// An observable sequence containing the value of the queried
        /// environment variable.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Environment.GetEnvironmentVariable(Variable));
        }

        /// <summary>
        /// Generates an observable sequence containing a string with the value
        /// of the queried environment variable, triggered with any input
        /// sequence
        /// </summary>
        /// <returns>
        /// An observable sequence containing the value of the queried
        /// environment variable.
        /// </returns>
        public IObservable<string> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => { return Environment.GetEnvironmentVariable(Variable); });
        }
    }
}
