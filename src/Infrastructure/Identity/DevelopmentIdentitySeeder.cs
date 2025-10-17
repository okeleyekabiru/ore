using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Infrastructure.Persistence;

namespace Ore.Infrastructure.Identity;

public static class DevelopmentIdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("DevelopmentIdentitySeeder");
        var adminSection = configuration.GetSection("Seed:Admin");
        if (!adminSection.Exists())
        {
            logger?.LogDebug("Seed:Admin configuration section is missing. Skipping development identity seeding.");
            return;
        }

        var email = adminSection.GetValue<string>("Email")?.Trim().ToLowerInvariant();
        var password = adminSection.GetValue<string>("Password");
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger?.LogWarning("Seed:Admin configuration is incomplete. Email and password are required.");
            return;
        }

        var firstName = adminSection.GetValue<string>("FirstName") ?? "Admin";
        var lastName = adminSection.GetValue<string>("LastName") ?? "User";

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var dateTimeProvider = services.GetRequiredService<IDateTimeProvider>();

        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning", StringComparison.Ordinal))
        {
            logger?.LogWarning(ex, "Skipping development identity seeding because the EF model has pending changes. Add a migration or update the model.");
            return;
        }

        foreach (var role in Enum.GetValues<RoleType>())
        {
            var roleName = role.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser is null)
        {
            var adminId = Guid.NewGuid();
            adminUser = new ApplicationUser
            {
                Id = adminId,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, password);
            if (!createResult.Succeeded)
            {
                var errorMessage = string.Join("; ", createResult.Errors.Select(error => error.Description));
                logger?.LogError("Failed to create development admin user: {Errors}", errorMessage);
                return;
            }

            logger?.LogInformation("Development admin identity user created with email {Email}.", email);
        }
        else
        {
            var normalizedEmail = email.ToUpperInvariant();
            if (!string.Equals(adminUser.NormalizedEmail, normalizedEmail, StringComparison.Ordinal))
            {
                adminUser.NormalizedEmail = normalizedEmail;
                adminUser.NormalizedUserName = normalizedEmail;
                await userManager.UpdateAsync(adminUser);
            }

            var hasPassword = await userManager.HasPasswordAsync(adminUser);
            if (hasPassword)
            {
                var removePasswordResult = await userManager.RemovePasswordAsync(adminUser);
                if (!removePasswordResult.Succeeded)
                {
                    var errors = string.Join("; ", removePasswordResult.Errors.Select(error => error.Description));
                    logger?.LogWarning("Unable to remove existing password for development admin {Email}: {Errors}", email, errors);
                }
            }

            var setPasswordResult = await userManager.AddPasswordAsync(adminUser, password);
            if (!setPasswordResult.Succeeded)
            {
                var errors = string.Join("; ", setPasswordResult.Errors.Select(error => error.Description));
                logger?.LogError("Failed to set development admin password for {Email}: {Errors}", email, errors);
            }
        }

        var existingRoles = await userManager.GetRolesAsync(adminUser);
        if (!existingRoles.Any(role => string.Equals(role, RoleType.Admin.ToString(), StringComparison.Ordinal)))
        {
            if (existingRoles.Count > 0)
            {
                await userManager.RemoveFromRolesAsync(adminUser, existingRoles);
            }

            await userManager.AddToRoleAsync(adminUser, RoleType.Admin.ToString());
            logger?.LogInformation("Assigned Admin role to development user {Email}.", email);
        }
        else if (existingRoles.Count > 1)
        {
            var rolesToRemove = existingRoles.Where(role => !string.Equals(role, RoleType.Admin.ToString(), StringComparison.Ordinal)).ToArray();
            if (rolesToRemove.Length > 0)
            {
                await userManager.RemoveFromRolesAsync(adminUser, rolesToRemove);
            }
        }

        var domainUsers = dbContext.Set<User>();
        var domainUser = await domainUsers.FirstOrDefaultAsync(user => user.Id == adminUser.Id, cancellationToken);
        if (domainUser is null)
        {
            domainUser = new User(adminUser.Id, email, firstName, lastName, RoleType.Admin)
            {
                CreatedOnUtc = dateTimeProvider.UtcNow,
                CreatedBy = adminUser.Id.ToString()
            };

            domainUsers.Add(domainUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Development admin domain user created for {Email}.", email);
        }
        else
        {
            domainUser.UpdateProfile(firstName, lastName);
            domainUser.ChangeRole(RoleType.Admin);
            domainUser.Activate();
            domainUser.ModifiedOnUtc = dateTimeProvider.UtcNow;
            domainUser.ModifiedBy = adminUser.Id.ToString();

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
