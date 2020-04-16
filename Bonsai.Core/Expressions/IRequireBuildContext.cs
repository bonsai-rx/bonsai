namespace Bonsai.Expressions
{
    interface IRequireBuildContext
    {
        IBuildContext BuildContext { get; set; }
    }
}
