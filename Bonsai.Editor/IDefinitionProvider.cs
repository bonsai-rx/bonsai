namespace Bonsai.Editor
{
    interface IDefinitionProvider
    {
        bool HasDefinition(object component);

        void ShowDefinition(object component);
    }
}
