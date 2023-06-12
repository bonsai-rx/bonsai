using System;
using System.Reflection;

namespace Bonsai.Editor
{
    [Serializable]
    public class TypeVisualizerDescriptor
    {
        public string VisualizerTypeName;
        public string TargetTypeName;

        public TypeVisualizerDescriptor(CustomAttributeData attribute)
        {
            if (attribute.ConstructorArguments.Count > 0)
            {
                var constructorArgument = attribute.ConstructorArguments[0];
                if (constructorArgument.ArgumentType.AssemblyQualifiedName == typeof(string).AssemblyQualifiedName)
                {
                    VisualizerTypeName = (string)constructorArgument.Value;
                }
                else VisualizerTypeName = ((Type)constructorArgument.Value).AssemblyQualifiedName;
            }

            for (int i = 0; i < attribute.NamedArguments.Count; i++)
            {
                var namedArgument = attribute.NamedArguments[i];
                switch (namedArgument.MemberName)
                {
                    case nameof(TypeVisualizerAttribute.TargetTypeName):
                        TargetTypeName = (string)namedArgument.TypedValue.Value;
                        break;
                    case nameof(TypeVisualizerAttribute.Target):
                        TargetTypeName = ((Type)namedArgument.TypedValue.Value).AssemblyQualifiedName;
                        break;
                }
            }
        }

        public TypeVisualizerDescriptor(TypeVisualizerAttribute typeVisualizer)
        {
            TargetTypeName = typeVisualizer.TargetTypeName;
            VisualizerTypeName = typeVisualizer.VisualizerTypeName;
        }
    }
}
