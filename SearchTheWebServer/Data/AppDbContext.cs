using Microsoft.EntityFrameworkCore;
using SearchTheWebServer.Models;

namespace SearchTheWebServer.Data;

public class AppDbContext : DbContext
{
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
{
    SearchLogs = Set<SearchLog>();
    Users = Set<User>();
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