using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class SurveyOptionConfiguration : IEntityTypeConfiguration<SurveyOption>
{
    public void Configure(EntityTypeBuilder<SurveyOption> builder)
    {
        builder.HasKey(so => so.Id);

        builder.Property(so => so.Value)
            .IsRequired()
            .HasMaxLength(500);
    }
}
