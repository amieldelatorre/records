using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts.ModelConfigurations;

namespace Persistence.Contexts;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new UserModelConfiguration().Configure(modelBuilder.Entity<User>());
    }
}