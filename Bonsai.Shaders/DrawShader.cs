using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Obsolete]
    [Description("Issues a shader draw call. This type is obsolete, please use DrawMesh instead.")]
    public class DrawShader : Sink
    {
        readonly DrawMesh drawMesh = new DrawMesh();

        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
        public string ShaderName
        {
            get { return drawMesh.ShaderName; }
            set { drawMesh.ShaderName = value; }
        }

        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to draw.")]
        public string MeshName
        {
            get { return drawMesh.MeshName; }
            set { drawMesh.MeshName = value; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return drawMesh.Process(source);
        }
    }
}
