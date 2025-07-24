using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Contexts.ModelConfigurations;

public class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .IsRequired()
            .HasColumnType("text");
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email_UNIQUE");
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasColumnType("text");
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasColumnType("text");
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("text");
        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasColumnType("text");
    }
}