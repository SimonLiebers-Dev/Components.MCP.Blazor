using System.ComponentModel;
using Components.MCP.Blazor.Introspection;
using DynamicHead.Blazor.Components;
using ModelContextProtocol.Server;

namespace Components.MCP.Blazor.Tools;

[McpServerToolType]
public sealed class ComponentTool
{
    private readonly ComponentMetadataProvider _provider = new(new ComponentDiscoveryOptions()
    {
        Assemblies = { typeof(DynamicHeadContent).Assembly }
    });

    [McpServerTool(Name = "components.list"), Description("List all available blazor components.")]
    public List<string> ListComponents()
    {
        var components = _provider.GetComponents();
        return components.Select(c => c.Name).ToList();
    }

    [McpServerTool(Name = "component.details"),
     Description("Shows details of a specific component by namspace and name of component.")]
    public ComponentMetadata? ShowComponentDetails(string componentNamespace, string componentName)
    {
        return _provider.GetComponent(componentNamespace, componentName);
    }
}