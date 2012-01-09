using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Dag
{
    public class DirectedGraphDescriptor<TValue, TLabel>
    {
        readonly Collection<TValue> nodes = new Collection<TValue>();
        readonly Collection<EdgeDescriptor<TLabel>> edges = new Collection<EdgeDescriptor<TLabel>>();

        public Collection<TValue> Nodes
        {
            get { return nodes; }
        }

        [XmlArrayItem("Edge")]
        public Collection<EdgeDescriptor<TLabel>> Edges
        {
            get { return edges; }
        }
    }
}
