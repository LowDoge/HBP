using FastEndpoints;
using FastEndpoints.OpenApi;
using HBP.Common;
using HBP.Data.Abstractions;
using HBP.Data.Postgres;
using HBP.Hotel.Application;
using HBP.Hotel.Infrastructure;
using HBP.Hotel.Infrastructure.Migrations;
using HBP.Messaging.Kafka;
using HBP.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHbpCommon();
builder.Services.AddHotelApplication();
builder.Services.AddHotelInfrastructure(builder.Configuration, "Redis");
builder.Services.AddHbpData(
    builder.Configuration,
    "HotelDb",
    typeof(CreateHotelsMigration).Assembly
);
builder.Services.AddHbpMessagePublishing();
builder.Services.AddHbpObservability(builder.Configuration, "hotel-api");

builder
    .Services.AddFastEndpoints()
    .OpenApiDocument(o =>
    {
        o.DocumentName = "v1";
        o.Title = "Hotel API";
        o.Version = "v1";
        o.MaxEndpointVersion = 1;
        o.EnableJWTBearerAuth = false;
    });

var app = builder.Build();

app.UseHbpObservability(app.Environment);

app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.RouteTemplate = "{version}";
    c.Versioning.DefaultVersion = 1;
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "v1");
        o.RoutePrefix = "swagger";
    });
}

await ApplyMigrationsAsync(app);

app.MapHbpObservability();

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    await migrationRunner.RunAsync().ConfigureAwait(false);
}

namespace HBP.Hotel.API
{
    public partial class Program;
}
