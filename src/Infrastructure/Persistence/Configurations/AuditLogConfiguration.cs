using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Actor)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.Entity)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.Metadata)
            .HasColumnType("jsonb");
    }
}
