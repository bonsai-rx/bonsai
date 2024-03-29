﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Osc.Net
{
    internal static class TransportManager
    {
        public const string DefaultConfigurationFile = "Osc.config";
        static readonly Dictionary<string, Tuple<ITransport, RefCountDisposable>> openConnections = new Dictionary<string, Tuple<ITransport, RefCountDisposable>>();
        static readonly object openConnectionsLock = new object();

        public static TransportDisposable ReserveConnection(string name)
        {
            return ReserveConnection(name, null);
        }

        internal static TransportDisposable ReserveConnection(string name, TransportConfiguration transportConfiguration)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A connection name must be specified.", "name");
            }

            Tuple<ITransport, RefCountDisposable> connection;
            lock (openConnectionsLock)
            {
                if (!openConnections.TryGetValue(name, out connection))
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(name))
                    {
                        transportConfiguration = configuration[name];
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    if (transportConfiguration == null)
                    {
                        throw new ArgumentException("The specified connection name has no matching configuration.");
                    }

                    var transport = transportConfiguration.CreateTransport();
                    var dispose = Disposable.Create(() =>
                    {
                        transport.Dispose();
                        openConnections.Remove(name);
                    });

                    var refCount = new RefCountDisposable(dispose);
                    connection = Tuple.Create(transport, refCount);
                    openConnections.Add(name, connection);
                    return new TransportDisposable(transport, refCount);
                }
            }

            return new TransportDisposable(connection.Item1, connection.Item2.GetDisposable());
        }

        [Obsolete]
        public static TransportConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new TransportConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(TransportConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (TransportConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        [Obsolete]
        public static void SaveConfiguration(TransportConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(TransportConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
