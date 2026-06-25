using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<GameSession> Sessions => Set<GameSession>();
    public DbSet<SessionActionLog> SessionActionLogs => Set<SessionActionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Name);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Wins).HasDefaultValue(0);
            entity.Property(e => e.Loses).HasDefaultValue(0);
            entity.Property(e => e.TotalGames).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");
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
            entity.Property(e => e.WinnerPlayerName).HasMaxLength(255);
            entity.Property(e => e.HitsThisTurn).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime(6)");
        });

        modelBuilder.Entity<SessionActionLog>(entity =>
        {
            entity.ToTable("session_action_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlayerName).HasMaxLength(255);
            entity.Property(e => e.ActionKind).HasMaxLength(64);
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.PayloadJson).HasColumnType("longtext");
            entity.HasIndex(e => new { e.SessionId, e.Sequence }).IsUnique();
        });
    }
}
