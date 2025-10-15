using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalRecordConfiguration : IEntityTypeConfiguration<ApprovalRecord>
{
    public void Configure(EntityTypeBuilder<ApprovalRecord> builder)
    {
        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Status)
            .HasConversion<int>();

        builder.Property(ar => ar.Status)
            .HasConversion<int>();

        builder.Property(ar => ar.Comments)
            .HasMaxLength(1000);

        builder.HasOne<ContentItem>()
            .WithMany(ci => ci.ApprovalHistory)
            .HasForeignKey(ar => ar.ContentItemId);
    }
}
