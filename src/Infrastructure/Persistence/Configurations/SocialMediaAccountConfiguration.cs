using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

public sealed class SocialMediaAccountConfiguration : IEntityTypeConfiguration<SocialMediaAccount>
{
    public void Configure(EntityTypeBuilder<SocialMediaAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TeamId)
            .IsRequired();

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.AccountName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.AccessToken)
            .IsRequired()
            .HasMaxLength(2000); // OAuth tokens can be quite long

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(2000);

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.LastUsedAt);

        // Ensure unique combination of team and platform
        builder.HasIndex(x => new { x.TeamId, x.Platform })
            .IsUnique()
            .HasDatabaseName("IX_SocialMediaAccounts_Team_Platform");

        builder.ToTable("SocialMediaAccounts");
    }
}