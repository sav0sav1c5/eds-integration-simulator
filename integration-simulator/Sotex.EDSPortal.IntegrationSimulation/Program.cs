using System.Net;
using Microsoft.OpenApi.Models;

string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var builder = WebApplication.CreateBuilder(args);

string configBasePath = environment == "Production" ? "/var/www/eds-portal" : AppContext.BaseDirectory;

builder.Configuration
    .SetBasePath(configBasePath)
    .AddJsonFile("appsettings.simulator.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.simulator.{environment}.json", optional: true)
    .AddEnvironmentVariables();

 
// Add services to the container.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
});

// builder.Services.AddControllers();
//builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger(c =>
{
    c.RouteTemplate = "integration-swagger/{documentName}/swagger.json";

    if(environment == "Production")
    {
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer> {
            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/integration-api" }
        };
    });
    }
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/integration-swagger/v1/swagger.json", "Integration Simulation API V1");
    // Don't set RoutePrefix to empty string - this would make it default
    c.RoutePrefix = "integration-swagger"; 
});
//}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers(); // This enables attribute routing for API controllers

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RequestStatus}/{action=Index}/{id?}");

app.Run();