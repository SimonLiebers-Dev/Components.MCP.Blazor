using Components.MCP.Blazor.Models;

namespace Components.MCP.Blazor.Introspection;

public interface IComponentMetadataProvider
{
    ComponentMetadata? GetComponent(string componentNamespace, string componentName);
    List<ComponentMetadata> GetComponents();
}