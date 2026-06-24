using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<GameSession> Sessions => Set<GameSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(32);
            entity.Property(e => e.CurrentTurn).HasMaxLength(32);
            entity.Property(e => e.CreatorPlayerName).HasMaxLength(255);
            entity.Property(e => e.RebelPlayerName).HasMaxLength(255);
            entity.Property(e => e.ImperialPlayerName).HasMaxLength(255);
            entity.Property(e => e.RebelBattleMapJson).HasColumnType("longtext");
            entity.Property(e => e.ImperialBattleMapJson).HasColumnType("longtext");
        });
    }
}
