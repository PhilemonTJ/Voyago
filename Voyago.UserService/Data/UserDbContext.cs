using Microsoft.EntityFrameworkCore;
using Voyago.UserService.Models;

namespace Voyago.UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OperatorProfile> OperatorProfiles => Set<OperatorProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(user => user.Phone)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.Property(user => user.UpdatedAt);

            entity.Property(user => user.DeletedAt);

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .IsRequired();

            entity.Property(token => token.ExpiresAt)
                .IsRequired();

            entity.Property(token => token.CreatedAt)
                .IsRequired();

            entity.Property(token => token.RevokedAt);

            entity.HasIndex(token => token.UserId);

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OperatorProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);

            entity.Property(profile => profile.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(profile => profile.Description);

            entity.Property(profile => profile.ContactNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(profile => profile.Address);

            entity.Property(profile => profile.LogoImageUrl);

            entity.Property(profile => profile.CreatedAt)
                .IsRequired();

            entity.Property(profile => profile.UpdatedAt);

            entity.HasIndex(profile => profile.UserId)
                .IsUnique();

            entity.HasOne(profile => profile.User)
                .WithOne(user => user.OperatorProfile)
                .HasForeignKey<OperatorProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}