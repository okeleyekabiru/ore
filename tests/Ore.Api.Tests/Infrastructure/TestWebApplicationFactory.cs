using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Infrastructure.Persistence;

namespace Ore.Api.Tests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Database=IntegrationTests;Username=test;Password=test",
                ["ConnectionStrings:Redis"] = "localhost",
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test",
                ["Jwt:SigningKey"] = "test-signing-key-test-signing-key"
            };

            config.AddInMemoryCollection(settings!);
        });

        builder.ConfigureTestServices(services =>
        {
            ReplaceDatabase(services);
            ReplaceCaching(services);
            ConfigureAuthentication(services);
        });
    }

    private static void ReplaceDatabase(IServiceCollection services)
    {
        var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(ApplicationDbContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        services.RemoveAll(typeof(IApplicationDbContext));
        services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("OreIntegrationTests");
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
    }

    private static void ReplaceCaching(IServiceCollection services)
    {
        services.RemoveAll(typeof(ICacheService));
        services.AddSingleton<ICacheService, InMemoryCacheService>();
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
    }
}
