namespace Components.MCP.Blazor.Introspection;

public interface IComponentMetadataProvider
{
    ComponentMetadata? GetComponent(string fullTypeName);
    List<ComponentMetadata> GetComponents();
}