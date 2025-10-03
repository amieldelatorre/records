using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Contexts.ModelConfigurations;

public class WeightEntryModelConfiguration : IEntityTypeConfiguration<WeightEntry>
{
    public void Configure(EntityTypeBuilder<WeightEntry> builder)
    {
        new CommonModelConfigurations<WeightEntry>().Configure(builder);
        builder.Property(w => w.Comment)
            .HasColumnType("text");
        builder.Property(w => w.EntryDate)
            .HasColumnType("date")
            .IsRequired();
        builder.Property(w => w.Value)
            .HasColumnType("real")
            .IsRequired();
        
        // Create an index making the combination of weight entry date and user id unique
        builder.HasIndex(w => new {w.EntryDate, w.UserId})
            .IsUnique()
            .HasDatabaseName("IX_WeightEntryDate_UserId_UNIQUE");
        
        builder.HasOne(w => w.OwningUser)
            .WithMany(u => u.WeightEntries)
            .HasForeignKey(w => w.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}