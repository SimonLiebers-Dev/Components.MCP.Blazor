using System.Text.Json.Serialization;

namespace Components.MCP.Blazor.Introspection;

/// <summary>
/// Represents a Blazor component and the parameters discovered via reflection.
/// </summary>
public sealed record ComponentMetadata(
    string Name,
    string Namespace,
    string Assembly,
    List<ComponentPropertyMetadata> Parameters,
    List<ComponentPropertyMetadata> CascadingParameters,
    List<ComponentPropertyMetadata> InjectedDependencies);

/// <summary>
/// Describes a component property such as a parameter, cascading parameter, or injected service.
/// </summary>
public sealed class ComponentPropertyMetadata(
    string name,
    string type,
    bool isRequired,
    bool captureUnmatchedValues,
    Dictionary<string, object?> traits)
{
    public static ComponentPropertyMetadata Create(
        string name,
        string type,
        bool isRequired = false,
        bool captureUnmatchedValues = false,
        IDictionary<string, object?>? traits = null)
    {
        var traitSource = traits is { Count: > 0 }
            ? new Dictionary<string, object?>(new Dictionary<string, object?>(traits))
            : [];

        return new ComponentPropertyMetadata(name, type, isRequired, captureUnmatchedValues, traitSource);
    }

    [JsonPropertyName("name")] public string Name { get; init; } = name;
    [JsonPropertyName("type")] public string Type { get; init; } = type;
    [JsonPropertyName("isRequired")] public bool IsRequired { get; init; } = isRequired;

    [JsonPropertyName("captureUnmatchedValues")]
    public bool CaptureUnmatchedValues { get; init; } = captureUnmatchedValues;

    [JsonIgnore] public Dictionary<string, object?> Traits { get; init; } = traits;

    public void Deconstruct(out string name, out string type, out bool isRequired, out bool captureUnmatchedValues,
        out Dictionary<string, object?> traits)
    {
        name = this.Name;
        type = this.Type;
        isRequired = this.IsRequired;
        captureUnmatchedValues = this.CaptureUnmatchedValues;
        traits = this.Traits;
    }
}