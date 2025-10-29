using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ore.Domain.Entities;

namespace Ore.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Team> Teams { get; }
    DbSet<BrandSurvey> BrandSurveys { get; }
    DbSet<SurveyQuestion> SurveyQuestions { get; }
    DbSet<SurveyOption> SurveyOptions { get; }
    DbSet<SurveyAnswer> SurveyAnswers { get; }
    DbSet<BrandSurveySubmission> BrandSurveySubmissions { get; }
    DbSet<ContentItem> ContentItems { get; }
    DbSet<ContentDistribution> ContentDistributions { get; }
    DbSet<ApprovalRecord> ApprovalRecords { get; }
    DbSet<ContentGenerationRequest> ContentGenerationRequests { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SocialMediaAccount> SocialMediaAccounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
