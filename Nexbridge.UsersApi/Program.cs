using Nexbridge.UsersApi.Application.Interfaces;
using Nexbridge.UsersApi.Application.Services;
using Nexbridge.UsersApi.Domain.Abstractions;
using Nexbridge.UsersApi.Infrastructure.Persistence;
using Nexbridge.UsersApi.Middleware;
using Microsoft.OpenApi.Models;

// Build and configure the web application host.
var builder = WebApplication.CreateBuilder(args);

// Register services used by the API.
// - In-memory repository as a singleton for process-lifetime state.
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Nexbridge Users API",
            Version = "v1"
        });
});
builder.Services.AddControllers();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Build the application pipeline.
var app = builder.Build();

// Expose OpenAPI only when running in development to avoid exposing
// internal API details in production profiles.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexbridge Users API v1");
    });
}

// API key authentication middleware.
// Configure Security:ApiKey (appsettings or environment variable Security__ApiKey)
// so every request is authorized with an X-Api-Key header.
app.UseExceptionHandling();
app.UseApiKeyAuthentication();

// Request + response logging middleware for observability.
app.UseRequestLogging();

// All requests should pass through HTTPS in hosted environments.
app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}
