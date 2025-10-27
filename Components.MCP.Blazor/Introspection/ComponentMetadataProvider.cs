using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Components.MCP.Blazor.Introspection;

public sealed class ComponentMetadataProvider(ComponentDiscoveryOptions? options = null)
{
    private static readonly Dictionary<Type, string> TypeAliases = new()
    {
        { typeof(void), "void" },
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(char), "char" },
        { typeof(string), "string" },
        { typeof(object), "object" }
    };

    private readonly ComponentDiscoveryOptions _options = options ?? new ComponentDiscoveryOptions();

    public List<ComponentMetadata> GetComponents()
    {
        var components = new List<ComponentMetadata>();
        var discoveredTypes = new HashSet<Type>();

        foreach (var assembly in ResolveAssemblies())
        {
            foreach (var type in GetLoadableTypes(assembly))
            {
                if (!IsComponent(type) || !discoveredTypes.Add(type))
                {
                    continue;
                }

                components.Add(CreateComponentMetadata(type));
            }
        }

        return components
            .OrderBy(component => component.Namespace, StringComparer.Ordinal)
            .ThenBy(component => component.Name, StringComparer.Ordinal)
            .ToList();
    }

    public ComponentMetadata? GetComponent(string componentNamespace, string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentNamespace))
            return null;

        if (string.IsNullOrWhiteSpace(componentName))
            return null;

        return GetComponents().FirstOrDefault(c =>
            c.Namespace.Equals(componentNamespace, StringComparison.OrdinalIgnoreCase) &&
            c.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<Assembly> ResolveAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        if (_options.IncludeEntryAssembly)
        {
            TryAdd(Assembly.GetEntryAssembly());
        }

        if (_options.IncludeExecutingAssembly)
        {
            TryAdd(Assembly.GetExecutingAssembly());
        }

        if (_options.IncludeLoadedAssemblies)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                TryAdd(assembly);
            }
        }

        foreach (var assembly in _options.Assemblies)
        {
            TryAdd(assembly);
        }

        foreach (var anchor in _options.AnchorComponentTypes)
        {
            TryAdd(anchor.Assembly);
        }

        return assemblies;

        void TryAdd(Assembly? assembly)
        {
            if (assembly is null)
            {
                return;
            }

            if (_options.AssemblyNameFilter is { } filter && !filter(assembly.GetName()))
            {
                return;
            }

            assemblies.Add(assembly);
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(t => t is not null).Select(t => t!);
        }
    }

    private static bool IsComponent(Type type) =>
        type is { IsAbstract: false, IsGenericTypeDefinition: false } &&
        typeof(ComponentBase).IsAssignableFrom(type);

    private static ComponentMetadata CreateComponentMetadata(Type componentType)
    {
        var parameterProperties = new List<ComponentPropertyMetadata>();
        var cascadingProperties = new List<ComponentPropertyMetadata>();
        var injectedProperties = new List<ComponentPropertyMetadata>();

        foreach (var property in GetUniqueReadableProperties(componentType))
        {
            var parameterAttribute = property.GetCustomAttribute<ParameterAttribute>(inherit: true);
            var cascadingParameterAttribute = property.GetCustomAttribute<CascadingParameterAttribute>(inherit: true);
            var injectAttribute = property.GetCustomAttribute<InjectAttribute>(inherit: true);

            if (parameterAttribute is null &&
                cascadingParameterAttribute is null &&
                injectAttribute is null)
            {
                continue;
            }

            var isRequired = property.IsDefined(typeof(EditorRequiredAttribute), inherit: true);
            var propertyTypeName = FormatTypeName(property.PropertyType);

            if (parameterAttribute is not null)
            {
                parameterProperties.Add(ComponentPropertyMetadata.Create(
                    property.Name,
                    propertyTypeName,
                    isRequired,
                    parameterAttribute.CaptureUnmatchedValues,
                    ExtractAttributeNames(property, includeCoreAttributes: true)));
            }

            if (cascadingParameterAttribute is not null)
            {
                cascadingProperties.Add(ComponentPropertyMetadata.Create(
                    property.Name,
                    propertyTypeName,
                    isRequired,
                    captureUnmatchedValues: false,
                    ExtractAttributeNames(property, includeCoreAttributes: true)));
            }

            if (injectAttribute is not null)
            {
                injectedProperties.Add(ComponentPropertyMetadata.Create(
                    property.Name,
                    propertyTypeName,
                    isRequired,
                    captureUnmatchedValues: false,
                    ExtractAttributeNames(property, includeCoreAttributes: false)));
            }
        }

        return new ComponentMetadata(
            componentType.Name,
            componentType.Namespace ?? string.Empty,
            componentType.Assembly.GetName().Name ?? componentType.Assembly.FullName ?? string.Empty,
            parameterProperties,
            cascadingProperties,
            injectedProperties);
    }

    private static IEnumerable<PropertyInfo> GetUniqueReadableProperties(Type componentType)
    {
        var properties =
            componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in properties)
        {
            if (!property.CanRead || property.IsSpecialName)
            {
                continue;
            }

            if (seen.Add(property.Name))
            {
                yield return property;
            }
        }
    }

    private static Dictionary<string, object?>? ExtractAttributeNames(PropertyInfo property,
        bool includeCoreAttributes)
    {
        var attributeNames = property
            .GetCustomAttributes(inherit: true)
            .Select(attribute => attribute.GetType())
            .Where(type => includeCoreAttributes || type != typeof(InjectAttribute))
            .Select(type => type.FullName ?? type.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return attributeNames.Length == 0
            ? null
            : new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Attributes"] = attributeNames
            };
    }

    private static string FormatTypeName(Type type)
    {
        if (TypeAliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsArray)
        {
            return $"{FormatTypeName(type.GetElementType()!)}[]";
        }

        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return $"{FormatTypeName(type.GenericTypeArguments[0])}?";
            }

            var builder = new StringBuilder();
            builder.Append(GetSimpleTypeName(type));
            builder.Append('<');
            builder.Append(string.Join(", ", type.GenericTypeArguments.Select(FormatTypeName)));
            builder.Append('>');
            return builder.ToString();
        }

        if (type is { IsNested: true, DeclaringType: not null })
        {
            return $"{FormatTypeName(type.DeclaringType)}.{GetSimpleTypeName(type)}";
        }

        return string.IsNullOrWhiteSpace(type.Namespace)
            ? GetSimpleTypeName(type)
            : $"{type.Namespace}.{GetSimpleTypeName(type)}";
    }

    private static string GetSimpleTypeName(Type type)
    {
        var name = type.Name;
        var backtickIndex = name.IndexOf('`');
        return backtickIndex > 0 ? name[..backtickIndex] : name;
    }
}