using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Contexts.ModelConfigurations;

public class CommonModelConfigurations<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .IsRequired()
            .HasColumnType("uuid");
        builder.Property(u => u.DateCreated)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        builder.Property(u => u.DateUpdated)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
    }
}