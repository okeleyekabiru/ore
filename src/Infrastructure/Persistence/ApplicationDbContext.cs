using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Domain.Common;
using Ore.Domain.Entities;
using Ore.Infrastructure.Identity;
using Ore.Infrastructure.Persistence.Configurations;

namespace Ore.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    DbSet<User> IApplicationDbContext.Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<BrandSurvey> BrandSurveys => Set<BrandSurvey>();
    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
    public DbSet<SurveyOption> SurveyOptions => Set<SurveyOption>();
    public DbSet<SurveyAnswer> SurveyAnswers => Set<SurveyAnswer>();
    public DbSet<BrandSurveySubmission> BrandSurveySubmissions => Set<BrandSurveySubmission>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentDistribution> ContentDistributions => Set<ContentDistribution>();
    public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();
    public DbSet<ContentGenerationRequest> ContentGenerationRequests => Set<ContentGenerationRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SocialMediaAccount> SocialMediaAccounts => Set<SocialMediaAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOnUtc = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOnUtc = utcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
