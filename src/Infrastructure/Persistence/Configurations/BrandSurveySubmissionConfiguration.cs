using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class BrandSurveySubmissionConfiguration : IEntityTypeConfiguration<BrandSurveySubmission>
{
    public void Configure(EntityTypeBuilder<BrandSurveySubmission> builder)
    {
        builder.HasKey(bss => bss.Id);

        builder.Property(bss => bss.SurveyId)
            .IsRequired();

        builder.Property(bss => bss.UserId)
            .IsRequired();

        builder.Navigation(bss => bss.Answers).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<SurveyAnswer>()
            .WithOne()
            .HasForeignKey(sa => sa.SubmissionId);
    }
}
