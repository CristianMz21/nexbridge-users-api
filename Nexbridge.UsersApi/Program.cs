using Nexbridge.UsersApi.Data;
using Nexbridge.UsersApi.Endpoints;

// Build and configure the web application host.
var builder = WebApplication.CreateBuilder(args);

// Register services used by the API.
// - OpenAPI endpoint metadata for local documentation.
// - In-memory repository as a singleton for process-lifetime state.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Build the application pipeline.
var app = builder.Build();

// Expose OpenAPI only when running in development to avoid exposing
// internal API details in production profiles.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// All requests should pass through HTTPS in hosted environments.
app.UseHttpsRedirection();

// Keep endpoint composition next to its feature code instead of keeping
// route registrations inside Program.cs.
app.MapUserEndpoints();

app.Run();
