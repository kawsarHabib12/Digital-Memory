using DigitalMemoryMap.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalMemoryMap.DAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Mood> Moods => Set<Mood>();
    public DbSet<Memory> Memories => Set<Memory>();
    public DbSet<MemoryPhoto> MemoryPhotos => Set<MemoryPhoto>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<MemoryTag> MemoryTags => Set<MemoryTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleId).ValueGeneratedOnAdd();
            entity.Property(r => r.RoleName).HasMaxLength(20).IsRequired();
            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Bio).HasMaxLength(300);
            entity.Property(u => u.ProfilePhotoPath).HasMaxLength(255);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasColumnType("datetime");
            entity.Property(u => u.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.IsActive).HasDefaultValue(true);
        });

        // Mood configuration
        modelBuilder.Entity<Mood>(entity =>
        {
            entity.ToTable("Moods");
            entity.HasKey(m => m.MoodId);
            entity.Property(m => m.MoodId).ValueGeneratedOnAdd();
            entity.Property(m => m.Name).HasMaxLength(30).IsRequired();
            entity.HasIndex(m => m.Name).IsUnique();
            entity.Property(m => m.Emoji).HasMaxLength(10).IsRequired();
        });

        // Memory configuration
        modelBuilder.Entity<Memory>(entity =>
        {
            entity.ToTable("Memories");
            entity.HasKey(m => m.MemoryId);
            entity.Property(m => m.Title).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Description).HasColumnType("nvarchar(max)");
            entity.Property(m => m.MemoryDate).HasColumnType("date");
            entity.Property(m => m.Latitude).HasPrecision(9, 6).IsRequired();
            entity.Property(m => m.Longitude).HasPrecision(9, 6).IsRequired();
            entity.Property(m => m.LocationName).HasMaxLength(200);
            entity.Property(m => m.Visibility).HasDefaultValue((byte)0);
            entity.Property(m => m.Status).HasDefaultValue((byte)1);
            entity.Property(m => m.CreatedAt).HasColumnType("datetime");
            entity.Property(m => m.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(m => m.User)
                .WithMany(u => u.Memories)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Category)
                .WithMany(c => c.Memories)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Mood)
                .WithMany(mood => mood.Memories)
                .HasForeignKey(m => m.MoodId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes according to PRD 8.2
            entity.HasIndex(m => new { m.UserId, m.MemoryDate });
            entity.HasIndex(m => new { m.UserId, m.CategoryId });
            entity.HasIndex(m => new { m.UserId, m.Latitude, m.Longitude });
        });

        // MemoryPhoto configuration
        modelBuilder.Entity<MemoryPhoto>(entity =>
        {
            entity.ToTable("MemoryPhotos");
            entity.HasKey(p => p.PhotoId);
            entity.Property(p => p.FilePath).HasMaxLength(255).IsRequired();
            entity.Property(p => p.OriginalFileName).HasMaxLength(200).IsRequired();
            entity.Property(p => p.FileSizeKb).IsRequired();
            entity.Property(p => p.IsCover).HasDefaultValue(false);
            entity.Property(p => p.UploadedAt).HasColumnType("datetime");

            entity.HasOne(p => p.Memory)
                .WithMany(m => m.Photos)
                .HasForeignKey(p => p.MemoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(t => t.TagId);
            entity.Property(t => t.Name).HasMaxLength(30).IsRequired();

            entity.HasOne(t => t.User)
                .WithMany(u => u.Tags)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tag names are unique per user
            entity.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
        });

        // MemoryTag junction table configuration
        modelBuilder.Entity<MemoryTag>(entity =>
        {
            entity.ToTable("MemoryTags");
            entity.HasKey(mt => new { mt.MemoryId, mt.TagId });

            entity.HasOne(mt => mt.Memory)
                .WithMany(m => m.MemoryTags)
                .HasForeignKey(mt => mt.MemoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mt => mt.Tag)
                .WithMany(t => t.MemoryTags)
                .HasForeignKey(mt => mt.TagId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Model-level Seed Data
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "User" },
            new Role { RoleId = 2, RoleName = "Admin" }
        );

        modelBuilder.Entity<Mood>().HasData(
            new Mood { MoodId = 1, Name = "Happy", Emoji = "😊" },
            new Mood { MoodId = 2, Name = "Loved", Emoji = "❤️" },
            new Mood { MoodId = 3, Name = "Funny", Emoji = "😂" },
            new Mood { MoodId = 4, Name = "Sad", Emoji = "😢" },
            new Mood { MoodId = 5, Name = "Excited", Emoji = "😍" },
            new Mood { MoodId = 6, Name = "Peaceful", Emoji = "😌" },
            new Mood { MoodId = 7, Name = "Normal", Emoji = "😐" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Travel", IsActive = true },
            new Category { CategoryId = 2, Name = "University", IsActive = true },
            new Category { CategoryId = 3, Name = "Family", IsActive = true },
            new Category { CategoryId = 4, Name = "Friends", IsActive = true },
            new Category { CategoryId = 5, Name = "Food", IsActive = true },
            new Category { CategoryId = 6, Name = "Events", IsActive = true },
            new Category { CategoryId = 7, Name = "Childhood", IsActive = true },
            new Category { CategoryId = 8, Name = "Nature", IsActive = true },
            new Category { CategoryId = 9, Name = "Other", IsActive = true }
        );
    }
}
