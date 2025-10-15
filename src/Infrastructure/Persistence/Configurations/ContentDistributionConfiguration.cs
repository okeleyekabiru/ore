using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class ContentDistributionConfiguration : IEntityTypeConfiguration<ContentDistribution>
{
    public void Configure(EntityTypeBuilder<ContentDistribution> builder)
    {
        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Platform)
            .HasConversion<int>();

        builder.Property(cd => cd.ExternalPostId)
            .HasMaxLength(200);

        builder.Property(cd => cd.FailureReason)
            .HasMaxLength(1000);

        builder.HasOne(cd => cd.ContentItem)
            .WithMany(ci => ci.Distributions)
            .HasForeignKey(cd => cd.ContentItemId);

        builder.OwnsOne(cd => cd.Window, ownedBuilder =>
        {
            ownedBuilder.Property(w => w.PublishOnUtc).HasColumnName("PublishOnUtc");
            ownedBuilder.Property(w => w.RetryInterval).HasColumnName("RetryInterval");
            ownedBuilder.Property(w => w.MaxRetryCount).HasColumnName("MaxRetryCount");
        });
    }
}
