using System.Collections.Generic;

namespace Bonsai.NuGet.Packaging
{
    public class BonsaiMetadata
    {
        public const string DefaultWorkflow = "$main";

        public string Version { get; set; }

        public Dictionary<string, WorkflowMetadata> Gallery { get; } = new();
    }

    public class WorkflowMetadata
    {
        public string Path { get; set; }
    }
}
