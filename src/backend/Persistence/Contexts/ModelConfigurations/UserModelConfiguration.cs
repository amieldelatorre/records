using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Contexts.ModelConfigurations;

public class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        new CommonModelConfigurations<User>().Configure(builder);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasColumnType("text");
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email_UNIQUE");
        builder.Property(x => x.Username)
            .IsRequired()
            .HasColumnType("text");
        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("IX_User_Username_UNIQUE");
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("text");
        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasColumnType("text");
        
        builder.HasMany(u => u.WeightEntries)
            .WithOne(w => w.OwningUser)
            .HasForeignKey(w => w.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}