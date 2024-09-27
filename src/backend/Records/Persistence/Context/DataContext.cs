using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public static class DatabaseIndexNames
{
    public const string UserEmailUnique = "IX_USER_EMAIL_UNIQUE";
}

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName(DatabaseIndexNames.UserEmailUnique);
    }
}