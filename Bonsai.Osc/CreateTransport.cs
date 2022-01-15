using Bonsai.Osc.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Osc
{
    /// <summary>
    /// Provides an abstract base class for operators that create OSC communication channels.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    public abstract class CreateTransport : Source<IDisposable>, INamedElement
    {
        readonly TransportConfiguration configuration;

        internal CreateTransport(TransportConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the name of the communication channel to reserve
        /// for the OSC protocol.
        /// </summary>
        [Description("The name of the communication channel to reserve for the OSC protocol.")]
        public string Name
        {
            get { return configuration.Name; }
            set { configuration.Name = value; }
        }

        /// <summary>
        /// Generates an observable sequence that contains an object representing
        /// an open connection to the Open Sound Control communication channel.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="IDisposable"/> class
        /// representing a connection to the underlying communication channel.
        /// </returns>
        public override IObservable<IDisposable> Generate()
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(configuration.Name, configuration),
                connection => Observable.Return(connection.Transport).Concat(Observable.Never(connection.Transport)));
        }
    }
}
