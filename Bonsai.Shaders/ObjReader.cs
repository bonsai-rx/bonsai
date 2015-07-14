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

        internal static void ReadObject(Shader shader, string fileName)
        {
            ushort vertexCount = 0;
            VertexAttribute position = null;
            VertexAttribute texCoord = null;
            VertexAttribute normals = null;
            var vertices = new List<float>();
            var indices = new List<ushort>();
            var indexMap = new Dictionary<Index, ushort>();
            foreach (var line in File.ReadAllLines(fileName))
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
                        for (int i = 1; i < values.Length; i++)
                        {
                            ushort index;
                            var face = Index.Create(values[i]);
                            if (!indexMap.TryGetValue(face, out index))
                            {
                                AddVertexAttribute(position, face.V, vertices);
                                AddVertexAttribute(texCoord, face.VT, vertices);
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

            shader.EnsureElementArray();
            shader.DrawMode = PrimitiveType.Triangles;
            shader.VertexCount = indices.Count;
            GL.BindVertexArray(shader.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, shader.VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, shader.ElementArray);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(vertices.Count * BlittableValueType<float>.Stride),
                          vertices.ToArray(),
                          BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          new IntPtr(indices.Count * BlittableValueType<ushort>.Stride),
                          indices.ToArray(),
                          BufferUsageHint.StaticDraw);
            if (position != null) offset = PushAttribArray(position, attrib++, stride, offset);
            if (texCoord != null) offset = PushAttribArray(texCoord, attrib++, stride, offset);
            if (normals != null) offset = PushAttribArray(normals, attrib++, stride, offset);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
