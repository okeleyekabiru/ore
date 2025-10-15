using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.HasKey(sq => sq.Id);

        builder.Property(sq => sq.Prompt)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sq => sq.Type)
            .HasConversion<int>();

        builder.Property(sq => sq.Order)
            .IsRequired();

        builder.Navigation(sq => sq.Options).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<SurveyOption>()
            .WithOne()
            .HasForeignKey(so => so.QuestionId);
    }
}
