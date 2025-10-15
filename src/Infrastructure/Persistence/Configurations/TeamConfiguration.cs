using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(t => t.BrandVoice, ownedBuilder =>
        {
            ownedBuilder.Property(bv => bv.Voice).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Tone).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Audience).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Keywords)
                .HasConversion(
                    v => string.Join(',', v ?? Array.Empty<string>()),
                    v => string.IsNullOrWhiteSpace(v)
                        ? Array.Empty<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToArray());
        });

        builder.Navigation(t => t.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Surveys).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Submissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.ContentItems).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(t => t.Members)
            .WithOne(u => u.Team)
            .HasForeignKey(u => u.TeamId);

        builder.HasMany(t => t.Surveys)
            .WithOne()
            .HasForeignKey(s => s.TeamId);
    }
}
