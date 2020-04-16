namespace Bonsai.Shaders
{
    struct MeshAttributeMapping
    {
        public Mesh Mesh;
        public int Divisor;

        public MeshAttributeMapping(Mesh mesh, int divisor)
        {
            Mesh = mesh;
            Divisor = divisor;
        }
    }
}
