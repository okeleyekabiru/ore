using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class BrandSurveyConfiguration : IEntityTypeConfiguration<BrandSurvey>
{
    public void Configure(EntityTypeBuilder<BrandSurvey> builder)
    {
        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(bs => bs.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(bs => bs.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(bs => bs.Questions)
            .WithOne()
            .HasForeignKey(sq => sq.SurveyId);
    }
}
