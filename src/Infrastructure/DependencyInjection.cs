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
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Jwt:Key"] ?? string.Empty))
                };
            });

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ILlmService, OpenAiLlmService>();
        services.AddScoped<IMediaStorageService, MinioStorageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISchedulingService, QuartzSchedulingService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.SectionName));

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddNpgSql(configuration.GetConnectionString("Database"))
            .AddRedis(configuration.GetConnectionString("Redis"));

        return services;
    }
}
