using Microsoft.EntityFrameworkCore;

namespace ObjectDiffNet.Extensions.EntityFramework.Tests;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestClass>().HasKey(te => te.Id);
    }

    public DbSet<TestClass> TestEntities { get; set; }
}