using OpenTK;

namespace Bonsai.Shaders.Rendering
{
    static class MatrixHelper
    {
        public static Matrix4 ToMatrix4(Assimp.Matrix4x4 transform)
        {
            ToMatrix4(ref transform, out Matrix4 result);
            return result;
        }

        public static void ToMatrix4(ref Assimp.Matrix4x4 transform, out Matrix4 result)
        {
            result.Row0 = new Vector4(transform.A1, transform.B1, transform.C1, transform.D1);
            result.Row1 = new Vector4(transform.A2, transform.B2, transform.C2, transform.D2);
            result.Row2 = new Vector4(transform.A3, transform.B3, transform.C3, transform.D3);
            result.Row3 = new Vector4(transform.A4, transform.B4, transform.C4, transform.D4);
        }
    }
}
