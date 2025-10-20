using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        var keywordsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (left, right) =>
                ReferenceEquals(left, right)
                || (left != null
                    && right != null
                    && left.SequenceEqual(right, StringComparer.OrdinalIgnoreCase)),
            collection => collection == null
                ? 0
                : collection.Aggregate(0, (hash, value) => HashCode.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(value ?? string.Empty))),
            collection => collection == null ? Array.Empty<string>() : collection.ToArray());

        builder.OwnsOne(t => t.BrandVoice, ownedBuilder =>
        {
            ownedBuilder.Property(bv => bv.Voice).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Tone).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Audience).HasMaxLength(200);
            ownedBuilder.Property(bv => bv.Goals).HasMaxLength(1000);
            ownedBuilder.Property(bv => bv.Competitors).HasMaxLength(1000);
            ownedBuilder.Property(bv => bv.Keywords)
                .HasConversion(
                    v => string.Join(',', v ?? Array.Empty<string>()),
                    v => string.IsNullOrWhiteSpace(v)
                        ? Array.Empty<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToArray())
                .Metadata.SetValueComparer(keywordsComparer);
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
