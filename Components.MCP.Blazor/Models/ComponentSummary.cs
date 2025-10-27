namespace Components.MCP.Blazor.Models;

public record ComponentSummary
{
    public string Name { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
}