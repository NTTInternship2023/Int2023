using Microsoft.EntityFrameworkCore;
using SearchTheWeb.Models;

namespace SearchTheWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } 
    public DbSet<SearchLog> SearchLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchLog>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.IdUser);
    }
}