using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ore.Application;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Llm;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Abstractions.Scheduling;
using Ore.Application.Abstractions.Storage;
using Ore.Infrastructure.Identity;
using Ore.Infrastructure.Persistence;
using Ore.Infrastructure.Options;
using Ore.Infrastructure.Services.Auditing;
using Ore.Infrastructure.Services.Caching;
using Ore.Infrastructure.Services.Llm;
using Ore.Infrastructure.Services.Messaging;
using Ore.Infrastructure.Services.Scheduling;
using Ore.Infrastructure.Services.Storage;
using Ore.Infrastructure.Services.Time;
using Quartz;
using Serilog;
using StackExchange.Redis;

namespace Ore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnection = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(databaseConnection);
        });

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = new SymmetricSecurityKey(jwtOptions.GetSigningKeyBytes());

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey
                };
                options.SaveToken = true;
            });

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ILlmService, OpenAiLlmService>();
        services.AddScoped<IMediaStorageService, MinioStorageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISchedulingService, QuartzSchedulingService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddQuartz();
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.SectionName));

        services.AddHealthChecks();

        return services;
    }
}
