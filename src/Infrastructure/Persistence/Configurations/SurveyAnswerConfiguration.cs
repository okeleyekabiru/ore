using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class SurveyAnswerConfiguration : IEntityTypeConfiguration<SurveyAnswer>
{
    public void Configure(EntityTypeBuilder<SurveyAnswer> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sa => sa.Metadata)
            .HasMaxLength(2000);
    }
}
