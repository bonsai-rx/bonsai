namespace Bonsai.Expressions
{
    class IncludeContext : GroupContext
    {
        string includePath;

        public IncludeContext(IBuildContext parentContext, string path)
            : base(parentContext)
        {
            includePath = path;
        }

        public string Path
        {
            get { return includePath; }
        }
    }
}
