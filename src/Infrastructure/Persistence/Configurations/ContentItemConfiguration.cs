using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Persistence.Configurations;

internal sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ci => ci.Body)
            .IsRequired();

        builder.Property(ci => ci.Caption)
            .HasMaxLength(500);

        builder.Property(ci => ci.Status)
            .HasConversion<int>();

        builder.HasOne(ci => ci.Author)
            .WithMany(u => u.AuthoredContent)
            .HasForeignKey(ci => ci.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        var hashtagsComparer = new ValueComparer<List<string>>(
            (left, right) =>
                ReferenceEquals(left, right)
                || (left != null
                    && right != null
                    && left.SequenceEqual(right, StringComparer.Ordinal)),
            collection => collection == null
                ? 0
                : collection.Aggregate(0, (hash, value) => HashCode.Combine(hash, StringComparer.Ordinal.GetHashCode(value ?? string.Empty))),
            collection => collection == null ? new List<string>() : collection.ToList());

        var hashtagsProperty = builder.Property<List<string>>("_hashtags")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Hashtags")
            .HasConversion(
                v => string.Join(',', v ?? new List<string>()),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList());

        hashtagsProperty.Metadata.SetValueComparer(hashtagsComparer);

        builder.Navigation(ci => ci.Distributions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(ci => ci.ApprovalHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
