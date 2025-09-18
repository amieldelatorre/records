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
        builder.Property(e => e.Value)
            .HasColumnType("real")
            .IsRequired();
        
        builder.HasOne(w => w.OwningUser)
            .WithMany(u => u.WeightEntries)
            .HasForeignKey(w => w.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);;
    }
}