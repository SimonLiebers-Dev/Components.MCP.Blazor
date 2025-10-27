using System.ComponentModel;
using Components.MCP.Blazor.Introspection;
using Components.MCP.Blazor.Models;
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
    public List<ComponentSummary> ListComponents()
    {
        var components = _provider.GetComponents();
        return components
            .Select(c => new ComponentSummary
            {
                Name = c.Name,
                Namespace = c.Namespace
            })
            .ToList();
    }

    [McpServerTool(Name = "component.details"),
     Description("Shows details of a specific component by its full name.")]
    public ComponentMetadata? ShowComponentDetails(string componentNamespace, string componentName)
    {
        return _provider.GetComponent(componentNamespace, componentName);
    }
}