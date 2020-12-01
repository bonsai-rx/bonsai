using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders.Rendering
{
    static class SceneMeshFactory
    {
        const int ElementSizeVector3D = 3;
        const int ElementSizeColor4D = 4;

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

            public void Add(float x, float y, float z)
            {
                var vertex = new Vector3(x, y, z);
                Vector3.ComponentMin(ref minimum, ref vertex, out minimum);
                Vector3.ComponentMax(ref maximum, ref vertex, out maximum);
            }
        }

        static int PushAttribArray(int index, int elementSize, int stride, int offset)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(
                index, elementSize,
                VertexAttribPointerType.Float,
                false, stride, offset);
            return offset + elementSize * BlittableValueType<float>.Stride;
        }

        static float[] GetVertices(Assimp.Mesh resource, out int stride, out VertexBounds bounds)
        {
            stride = 0;
            bounds = new VertexBounds();
            if (resource.HasVertices) stride += ElementSizeVector3D;
            for (int i = 0; i < resource.TextureCoordinateChannelCount; i++)
            {
                stride += resource.UVComponentCount[i];
            }
            stride += resource.VertexColorChannelCount * ElementSizeColor4D;
            if (resource.HasNormals) stride += ElementSizeVector3D;

            var vertices = new float[resource.VertexCount * stride];
            for (int i = 0, v = 0; i < resource.VertexCount; i++)
            {
                if (resource.HasVertices)
                {
                    var vertex = resource.Vertices[i];
                    vertices[v++] = vertex.X;
                    vertices[v++] = vertex.Y;
                    vertices[v++] = vertex.Z;
                    bounds.Add(vertex.X, vertex.Y, vertex.Z);
                }

                for (int k = 0; k < resource.TextureCoordinateChannelCount; k++)
                {
                    var texCoord = resource.TextureCoordinateChannels[k][i];
                    vertices[v++] = texCoord.X;
                    vertices[v++] = texCoord.Y;
                    if (resource.UVComponentCount[k] > 2)
                    {
                        vertices[v++] = texCoord.Z;
                    }
                }

                for (int c = 0; c < resource.VertexColorChannelCount; c++)
                {
                    var color = resource.VertexColorChannels[c][i];
                    vertices[v++] = color.R;
                    vertices[v++] = color.G;
                    vertices[v++] = color.B;
                    vertices[v++] = color.A;
                }

                if (resource.HasNormals)
                {
                    var normal = resource.Normals[i];
                    vertices[v++] = normal.X;
                    vertices[v++] = normal.Y;
                    vertices[v++] = normal.Z;
                }
            }

            stride *= BlittableValueType<float>.Stride;
            return vertices;
        }

        public static SceneMesh CreateMesh(Assimp.Mesh resource)
        {
            var mesh = new SceneMesh();
            mesh.MaterialIndex = resource.MaterialIndex;
            var vertices = GetVertices(resource, out int stride, out VertexBounds bounds);
            mesh.Bounds = new Bounds(bounds.Center, bounds.Extents);
            mesh.VertexCount = resource.VertexCount;
            mesh.DrawMode = PrimitiveType.Triangles;
            GL.BindVertexArray(mesh.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          new IntPtr(vertices.Length * BlittableValueType<float>.Stride),
                          vertices, BufferUsageHint.StaticDraw);
            if (resource.HasFaces)
            {
                mesh.EnsureElementArray();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ElementArray);
                if (resource.VertexCount <= short.MaxValue)
                {
                    var indices = resource.GetShortIndices();
                    mesh.VertexCount = indices.Length;
                    mesh.ElementArrayType = DrawElementsType.UnsignedShort;
                    GL.BufferData(BufferTarget.ElementArrayBuffer,
                                  new IntPtr(indices.Length * BlittableValueType<short>.Stride),
                                  indices, BufferUsageHint.StaticDraw);
                }
                else
                {
                    var indices = resource.GetIndices();
                    mesh.VertexCount = indices.Length;
                    mesh.ElementArrayType = DrawElementsType.UnsignedInt;
                    GL.BufferData(BufferTarget.ElementArrayBuffer,
                                  new IntPtr(indices.Length * BlittableValueType<int>.Stride),
                                  indices, BufferUsageHint.StaticDraw);
                }
            }

            var attrib = 0;
            var offset = 0;
            if (resource.HasVertices) offset = PushAttribArray(attrib++, ElementSizeVector3D, stride, offset);
            for (int k = 0; k < resource.TextureCoordinateChannelCount; k++)
            {
                offset = PushAttribArray(attrib++, resource.UVComponentCount[k], stride, offset);
            }
            for (int c = 0; c < resource.VertexColorChannelCount; c++)
            {
                offset = PushAttribArray(attrib++, ElementSizeColor4D, stride, offset);
            }
            if (resource.HasNormals) PushAttribArray(attrib++, ElementSizeVector3D, stride, offset);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            return mesh;
        }
    }
}
