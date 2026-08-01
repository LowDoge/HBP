using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace HBP.Observability;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHbpObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var seqUrl = configuration["Observability:Seq:Url"];
        var otlpEndpoint = configuration["Observability:OpenTelemetry:Endpoint"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("service", serviceName)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(opts =>
            {
                opts.IncludedData = IncludedData.SpanIdField | IncludedData.TraceIdField;
                opts.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serviceName,
                };
            })
            .WriteTo.Seq(seqUrl ?? string.Empty)
            .CreateLogger();

        services.AddSerilog();

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName: serviceName, serviceVersion: "1.0.0")
            )
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                }
            });

        services.AddHealthChecks();

        return services;
    }

    public static IEndpointRouteBuilder MapHbpObservability(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMetrics("/metrics");
        endpoints.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions { Predicate = _ => false, ResponseWriter = HealthResponseWriter }
        );
        endpoints.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthResponseWriter,
            }
        );

        return endpoints;
    }

    public static IApplicationBuilder UseHbpObservability(
        this IApplicationBuilder app,
        IHostEnvironment env
    )
    {
        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("RemoteIp", http.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
            };
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        return app;
    }

    private static Task HealthResponseWriter(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), description = e.Value.Description }
            ),
        };
        return context.Response.WriteAsJsonAsync(payload);
    }
}
