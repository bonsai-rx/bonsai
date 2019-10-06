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
        readonly MeshAttributeMapping[] meshAttributes;

        internal MeshAggregate(IEnumerable<MeshAttributeMapping> source)
        {
            meshAttributes = source.ToArray();
            GL.GenVertexArrays(1, out vao);

            var indexOffset = 0;
            var vertexAttributes = new List<InstanceAttributeMapping>();
            foreach (var mapping in meshAttributes)
            {
                var vertexStride = 0;
                GL.BindVertexArray(mapping.Mesh.VertexArray);
                for (int i = 0; ; i++)
                {
                    int enabled;
                    GL.GetVertexAttrib(i, VertexAttribParameter.ArrayEnabled, out enabled);
                    if (enabled == 0) break;

                    int size, type, normalized, vstride;
                    var attribute = new InstanceAttributeMapping();
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
                    attribute.Divisor = mapping.Divisor;
                    vertexAttributes.Add(attribute);
                }

                var offset = 0;
                GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mapping.Mesh.VertexBuffer);
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
                    if (attribute.Divisor > 0) GL.VertexAttribDivisor(index, attribute.Divisor);
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
            get { return meshAttributes.Select(mapping => mapping.Mesh); }
        }

        public int VertexArray
        {
            get { return vao; }
        }

        public void Draw()
        {
            var eao = 0;
            var vertexCount = -1;
            var instanceCount = -1;
            var drawMode = default(PrimitiveType);
            var elementArrayType = DrawElementsType.UnsignedShort;
            for (int i = 0; i < meshAttributes.Length; i++)
            {
                var mesh = meshAttributes[i].Mesh;
                var divisor = meshAttributes[i].Divisor;
                if (divisor > 0)
                {
                    if (mesh.ElementArray > 0)
                    {
                        throw new NotSupportedException("Only non-instance data meshes can specify an element array buffer when aggregating.");
                    }

                    var count = mesh.VertexCount * divisor;
                    if (instanceCount >= 0 && count != instanceCount)
                    {
                        throw new NotSupportedException("All aggregated instance data must specify the same number of instances.");
                    }

                    instanceCount = count;
                }
                else
                {
                    var mode = mesh.DrawMode;
                    if (vertexCount >= 0 && mode != drawMode)
                    {
                        throw new NotSupportedException("All aggregated vertex data must specify the same draw mode.");
                    }

                    var count = mesh.VertexCount;
                    if (vertexCount >= 0 && count != vertexCount)
                    {
                        throw new NotSupportedException("All aggregated vertex data must specify the same number of vertices.");
                    }

                    drawMode = mode;
                    vertexCount = count;
                    if (mesh.ElementArray > 0)
                    {
                        if (eao > 0)
                        {
                            throw new NotSupportedException("Aggregated mesh data can specify only a single element array buffer.");
                        }
                        eao = mesh.ElementArray;
                        elementArrayType = mesh.ElementArrayType;
                    }
                }
            }

            if (vertexCount > 0 && instanceCount > 0)
            {
                GL.BindVertexArray(vao);
                if (eao > 0)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);
                    GL.DrawElementsInstanced(drawMode, vertexCount, elementArrayType, IntPtr.Zero, instanceCount);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }
                else GL.DrawArraysInstanced(drawMode, 0, vertexCount, instanceCount);
                GL.BindVertexArray(0);
            }
            else if (vertexCount > 0 && instanceCount < 0)
            {
                GL.BindVertexArray(vao);
                if (eao > 0)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);
                    GL.DrawElements(drawMode, vertexCount, elementArrayType, IntPtr.Zero);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }
                else GL.DrawArrays(drawMode, 0, vertexCount);
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, ref vao);
        }
    }
}
