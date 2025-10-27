using System.Reflection;

namespace Components.MCP.Blazor.Introspection;

public class ComponentDiscoveryOptions
{
    public List<Assembly> Assemblies { get; } = [];

    public List<Type> AnchorComponentTypes { get; } = [];

    public bool IncludeEntryAssembly { get; set; } = true;

    public bool IncludeExecutingAssembly { get; set; } = true;

    public bool IncludeLoadedAssemblies { get; set; } = true;

    public Func<AssemblyName, bool>? AssemblyNameFilter { get; set; }
}