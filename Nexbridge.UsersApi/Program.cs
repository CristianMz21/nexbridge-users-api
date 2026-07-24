using Nexbridge.UsersApi.Application.Interfaces;
using Nexbridge.UsersApi.Application.Services;
using Nexbridge.UsersApi.Domain.Abstractions;
using Nexbridge.UsersApi.Infrastructure.Persistence;
using Nexbridge.UsersApi.Middleware;

// Build and configure the web application host.
var builder = WebApplication.CreateBuilder(args);

// Register services used by the API.
// - OpenAPI endpoint metadata for local documentation.
// - In-memory repository as a singleton for process-lifetime state.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Build the application pipeline.
var app = builder.Build();

// Expose OpenAPI only when running in development to avoid exposing
// internal API details in production profiles.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Optional API key authentication middleware. Configure Security:ApiKey
// in appsettings when you want to enforce this locally or in production.
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
