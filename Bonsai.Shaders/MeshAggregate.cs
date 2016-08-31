using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MeshAggregate : IDisposable
    {
        int vao;
        readonly IEnumerable<Mesh> meshes;

        internal MeshAggregate(IEnumerable<Mesh> source)
        {
            meshes = source;
            GL.GenVertexArrays(1, out vao);

            var indexOffset = 0;
            var vertexAttributes = new List<VertexAttributeMapping>();
            foreach (var mesh in meshes)
            {
                var vertexStride = 0;
                GL.BindVertexArray(mesh.VertexArray);
                for (int i = 0; ; i++)
                {
                    int enabled;
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArrayEnabled, out enabled);
                    if (enabled == 0) break;

                    int size, type, normalized, vstride;
                    var attribute = new VertexAttributeMapping();
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArraySize, out size);
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArrayType, out type);
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArrayNormalized, out normalized);
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArrayStride, out vstride);
                    if (vertexStride == 0) vertexStride = vstride;
                    else if (vstride != vertexStride)
                    {
                        throw new InvalidOperationException("Meshes with interleaved vertex buffers are not supported.");
                    }

                    attribute.Size = size;
                    attribute.Type = (VertexAttribPointerType)type;
                    attribute.Normalized = normalized != 0 ? true : false;
                    vertexAttributes.Add(attribute);
                }

                var offset = 0;
                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VertexBuffer);
                for (int i = 0; i < vertexAttributes.Count; i++)
                {
                    var index = i + indexOffset;
                    var attribute = vertexAttributes[i];
                    GL.EnableVertexAttribArray(index);
                    GL.VertexAttribPointer(
                        index, attribute.Size,
                        attribute.Type,
                        attribute.Normalized,
                        vertexStride, offset);
                    offset += attribute.Size * VertexHelper.GetVertexAttributeSize(attribute.Type);
                }

                indexOffset += vertexAttributes.Count;
                vertexAttributes.Clear();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public IEnumerable<Mesh> Meshes
        {
            get { return meshes; }
        }

        public int VertexArray
        {
            get { return vao; }
        }

        public void Draw()
        {
            var drawMode = meshes.Select(mesh => mesh.DrawMode).Distinct().Single();
            var vertexCount = meshes.Select(mesh => mesh.VertexCount).Distinct().Single();
            if (vertexCount > 0)
            {
                GL.BindVertexArray(vao);
                GL.DrawArrays(drawMode, 0, vertexCount);
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, ref vao);
        }
    }
}
