using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(a => a.AccountNumber).IsUnique();
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(a => a.UserId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId);
        });
    }
}
