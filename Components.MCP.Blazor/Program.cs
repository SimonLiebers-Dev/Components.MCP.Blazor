using Components.MCP.Blazor.Introspection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Components.MCP.Blazor.Tools;
using DynamicHead.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

var discoveryOptions = new ComponentDiscoveryOptions();
discoveryOptions.Assemblies.Add(typeof(DynamicHeadContent).Assembly);

builder.Services.AddSingleton(discoveryOptions);
builder.Services.AddSingleton<IComponentMetadataProvider, ComponentMetadataProvider>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<ComponentTool>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();

var app = builder.Build();

app.MapMcp();

app.Run();