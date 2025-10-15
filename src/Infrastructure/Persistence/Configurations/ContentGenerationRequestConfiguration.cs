using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class ContentGenerationRequestConfiguration : IEntityTypeConfiguration<ContentGenerationRequest>
{
    public void Configure(EntityTypeBuilder<ContentGenerationRequest> builder)
    {
        builder.HasKey(cgr => cgr.Id);

        builder.Property(cgr => cgr.Status)
            .HasConversion<int>();

        builder.Property(cgr => cgr.Prompt)
            .IsRequired();

        builder.Property(cgr => cgr.Model)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cgr => cgr.RawResponse)
            .HasColumnType("text");

        builder.HasOne(cgr => cgr.ContentItem)
            .WithOne()
            .HasForeignKey<ContentGenerationRequest>(cgr => cgr.ContentItemId);
    }
}
