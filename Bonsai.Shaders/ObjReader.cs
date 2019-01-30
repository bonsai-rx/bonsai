using Bonsai.Shaders.Configuration;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ObjReader
    {
        class VertexBounds
        {
            Vector3 minimum = Vector3.One * float.MaxValue;
            Vector3 maximum = Vector3.One * float.MinValue;

            public Vector3 Center
            {
                get { return (maximum + minimum) / 2; }
            }

            public Vector3 Extents
            {
                get { return (maximum - minimum) / 2; }
            }

            public void Add(ref Vector3 vertex)
            {
                Vector3.ComponentMin(ref minimum, ref vertex, out minimum);
                Vector3.ComponentMax(ref maximum, ref vertex, out maximum);
            }
        }

        class VertexAttribute : List<float>
        {
            public int ElementSize;
        }

        struct Index
        {
            public int V;
            public int VT;
            public int VN;

            public static Index Create(string face)
            {
                var index = new Index();
                var values = face.Split('/');
                for (int i = 0; i < values.Length; i++)
                {
                    int value;
                    if (string.IsNullOrEmpty(values[i])) continue;
                    if (!int.TryParse(values[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        throw new InvalidOperationException(string.Format(
                            "Invalid face specification: {0}.",
                            values[i]));
                    }

                    if (i == 0) index.V = value;
                    if (i == 1) index.VT = value;
                    if (i == 2) index.VN = value;
                }

                return index;
            }
        }

        static void ParseValues(ref VertexAttribute buffer, string[] values)
        {
            if (buffer == null)
            {
                buffer = new VertexAttribute();
                buffer.ElementSize = values.Length - 1;
            }
            else if (buffer.ElementSize != values.Length - 1)
            {
                throw new InvalidOperationException("Invalid vertex specification. Vertex attributes must all have the same size.");
            }

            for (int i = 1; i < values.Length; i++)
            {
                float value;
                if (!float.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid vertex specification: {0}.",
                        values[i]));
                }

                buffer.Add(value);
            }
        }

        static void AddVertexAttribute(VertexAttribute buffer, int index, List<float> vertices)
        {
            if (buffer == null && index <= 0) return;
            else if (buffer == null || index <= 0)
            {
                throw new InvalidOperationException("Invalid face specification. Specified vertex attribute does not exist.");
            }

            var offset = (index - 1) * buffer.ElementSize;
            for (int i = offset; i < offset + buffer.ElementSize; i++)
            {
                vertices.Add(buffer[i]);
            }
        }

        static void UpdateVertexBounds(VertexAttribute buffer, int index, VertexBounds bounds)
        {
            if (buffer == null && index <= 0) return;
            if (bounds == null)
            {
                bounds = new VertexBounds();
            }

            Vector3 vertex;
            var offset = (index - 1) * buffer.ElementSize;
            vertex.X = buffer.ElementSize > 0 ? buffer[offset + 0] : 0;
            vertex.Y = buffer.ElementSize > 1 ? buffer[offset + 1] : 0;
            vertex.Z = buffer.ElementSize > 2 ? buffer[offset + 2] : 0;
            bounds.Add(ref vertex);
        }

        static int GetElementSize(VertexAttribute buffer)
        {
            return buffer == null ? 0 : buffer.ElementSize;
        }

        static int PushAttribArray(VertexAttribute buffer, int index, int stride, int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(
                index, buffer.ElementSize,
                VertexAttribPointerType.Float,
                false, stride, offset);
            return offset + buffer.ElementSize * BlittableValueType<float>.Stride;
        }

        internal static void ReadObject(Mesh mesh, StreamReader stream)
        {
            string line;
            var faceLength = 0;
            uint vertexCount = 0;
            VertexBounds bounds = new VertexBounds();
            VertexAttribute position = null;
            VertexAttribute texCoord = null;
            VertexAttribute normals = null;
            var vertices = new List<float>();
            var indices = new List<uint>();
            var indexMap = new Dictionary<Index, uint>();
            while ((line = stream.ReadLine()) != null)
            {
                var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0) continue;
                switch (values[0])
                {
                    case "v":
                        ParseValues(ref position, values);
                        break;
                    case "vt":
                        ParseValues(ref texCoord, values);
                        break;
                    case "vn":
                        ParseValues(ref normals, values);
                        break;
                    case "f":
                        var length = values.Length - 1;
                        if (faceLength == 0) faceLength = length;
                        else if (faceLength != length)
                        {
                            throw new InvalidOperationException("Invalid face specification. All faces must have the same number of vertices.");
                        }

                        for (int i = 1; i < values.Length; i++)
                        {
                            uint index;
                            var face = Index.Create(values[i]);
                            if (!indexMap.TryGetValue(face, out index))
                            {
                                AddVertexAttribute(position, face.V, vertices);
                                UpdateVertexBounds(position, face.V, bounds);
                                if (texCoord != null)
                                {
                                    AddVertexAttribute(texCoord, face.VT, vertices);
                                }
                                AddVertexAttribute(normals, face.VN, vertices);
                                index = vertexCount++;
                                indexMap.Add(face, index);
                            }

                            indices.Add(index);
                        }
                        break;
                    default:
                        continue;
                }
            }

            var attrib = 0;
            var offset = 0;
            var stride = GetElementSize(position) + GetElementSize(texCoord) + GetElementSize(normals);
            stride = stride * BlittableValueType<float>.Stride;

            mesh.EnsureElementArray();
            mesh.VertexCount = indices.Count;
            mesh.Bounds = new Bounds(bounds.Center, bounds.Extents);
            mesh.DrawMode = faceLength == 4 ? PrimitiveType.Quads : PrimitiveType.Triangles;
            GL.BindVertexArray(mesh.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ElementArray);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(vertices.Count * BlittableValueType<float>.Stride),
                          vertices.ToArray(),
                          BufferUsageHint.StaticDraw);
            if (vertexCount <= byte.MaxValue)
            {
                mesh.ElementArrayType = DrawElementsType.UnsignedByte;
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              new IntPtr(indices.Count * BlittableValueType<byte>.Stride),
                              indices.Select(x => (byte)x).ToArray(),
                              BufferUsageHint.StaticDraw);
            }
            else if (vertexCount <= ushort.MaxValue)
            {
                mesh.ElementArrayType = DrawElementsType.UnsignedShort;
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              new IntPtr(indices.Count * BlittableValueType<ushort>.Stride),
                              indices.Select(x => (ushort)x).ToArray(),
                              BufferUsageHint.StaticDraw);
            }
            else
            {
                mesh.ElementArrayType = DrawElementsType.UnsignedInt;
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              new IntPtr(indices.Count * BlittableValueType<uint>.Stride),
                              indices.ToArray(),
                              BufferUsageHint.StaticDraw);
            }

            if (position != null) offset = PushAttribArray(position, attrib++, stride, offset);
            if (texCoord != null) offset = PushAttribArray(texCoord, attrib++, stride, offset);
            if (normals != null) offset = PushAttribArray(normals, attrib++, stride, offset);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
