using Microsoft.EntityFrameworkCore;

namespace _9GagClone.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.UserId, f.FriendId });

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Friend)
            .WithMany(u => u.FriendOf)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}