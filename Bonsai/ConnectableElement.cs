using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public class ConnectableElement : LoadableElement
    {
        IDisposable connection;
        Func<IDisposable> connect;

        public ConnectableElement(Func<IDisposable> connector)
        {
            connect = connector;
        }

        public override IDisposable Load()
        {
            connection = connect();
            return base.Load();
        }

        protected override void Unload()
        {
            connection.Dispose();
            base.Unload();
        }
    }
}
