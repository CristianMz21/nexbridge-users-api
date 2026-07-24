using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Nexbridge.UsersApi.Tests.Testing;

public sealed class UserApiApplicationFactory(string? apiKey = null, string? environment = null)
    : WebApplicationFactory<Program>
{
    private readonly string? _environment = environment;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment ?? "Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Security:ApiKey"] = string.Empty
                    });

                return;
            }

            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Security:ApiKey"] = apiKey
                });
        });
    }
}
