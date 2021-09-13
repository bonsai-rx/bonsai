namespace Bonsai.Shaders.Rendering
{
    static class ShaderConstants
    {
        public static readonly string ModelViewMatrix = "modelview";
        public static readonly string ProjectionMatrix = "projection";
        public static readonly string NormalMatrix = "normalMatrix";
        public static readonly string Texture = "texture";

        public static readonly string BumpScaling = GetUniformName(nameof(Assimp.Material.BumpScaling));
        public static readonly string ColorAmbient = GetUniformName(nameof(Assimp.Material.ColorAmbient));
        public static readonly string ColorDiffuse = GetUniformName(nameof(Assimp.Material.ColorDiffuse));
        public static readonly string ColorEmissive = GetUniformName(nameof(Assimp.Material.ColorEmissive));
        public static readonly string ColorReflective = GetUniformName(nameof(Assimp.Material.ColorReflective));
        public static readonly string ColorSpecular = GetUniformName(nameof(Assimp.Material.ColorSpecular));
        public static readonly string ColorTransparent = GetUniformName(nameof(Assimp.Material.ColorTransparent));
        public static readonly string Opacity = GetUniformName(nameof(Assimp.Material.Opacity));
        public static readonly string Reflectivity = GetUniformName(nameof(Assimp.Material.Reflectivity));
        public static readonly string Shininess = GetUniformName(nameof(Assimp.Material.Shininess));
        public static readonly string ShininessStrength = GetUniformName(nameof(Assimp.Material.ShininessStrength));

        static string GetUniformName(string resourceName)
        {
            if (resourceName.Length < 1) return resourceName;
            if (resourceName.Length == 1) return resourceName.ToLowerInvariant();
            return resourceName.Substring(0, 1).ToLowerInvariant() + resourceName.Substring(1);
        }
    }
}
